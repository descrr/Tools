using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Luxottica.AdamExtensions.Maintenance.Jobs.DownloadViaFileConfiguration.Excel;
using Luxottica.AdamExtensions.Maintenance.Jobs.DownloadViaFileConfiguration.Helpers;
using Microsoft.Office.Interop.Excel;

namespace PriceGenerator
{
	public class ProductCollector
	{
		private string TargetPricePath;
		private string SourceTxtFile;
		private string Urls2Update;
		private static string WebPageContent;
		private int? LastModificationId = null;
		private int NewRecords = 0;
		private int UpdatedPriceFields = 0;
		private int DeletedRecords = 0;
		private string LogMessages = string.Empty;

		public ProductCollector()
		{
			string directory = Directory.GetCurrentDirectory();
			for (int i = 0; i < 4; i++)
				directory = GetParentDirectory(directory);

			SourceTxtFile = String.Format(@"{0}\SitemapsCorrector\Vofisplus\sitemap.txt", directory);
			Urls2Update = String.Format(@"{0}\PriceGenerator\Price\URLs.xls", directory);
			TargetPricePath = String.Format(@"{0}\PriceGenerator\Price\TargetPrice\price_vofisplus.xls", directory);
		}

		private string GetParentDirectory(string initialDirectory)
		{
			return Directory.GetParent(initialDirectory).FullName;
		}

		public void RenewPrice(bool reloadAllProducts)
		{
			if (reloadAllProducts)
			{
				ReloadAllProducts();
			}

            var priceAnalyzer = new PriceAnalyzer();
            priceAnalyzer.AnalysePrice();

            GenerateExcelPrice();
			SyncPrice();
		}

		public bool IsPriceModified()
		{
			bool modified = (LastModificationId != GetLastModificationId());
			if (modified)
				return modified;

			using (var connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();
				string sqlSelect = "SELECT 1 FROM [dbo].[PriceMarket] where isPrice <> isNextPrice";
				SqlCommand command = new SqlCommand(sqlSelect, connection);
				SqlDataReader reader = command.ExecuteReader();

				if (reader.Read())
					modified = true;
			}

			return modified;
		}

		private void SendReport()
		{
			if (!IsPriceModified())
				return;

			LogMessage("");
			LogMessage("Report");
			LogMessage(string.Format("NewRecords: {0}, UpdatedFields={1}, DeletedRecords={2}", NewRecords, UpdatedPriceFields, DeletedRecords));

			var mailSender = new MailSender();
			mailSender.SendMail(LogMessages, new List<string>() { TargetPricePath });
		}

		private bool IsUrlExists(string url)
		{
			bool exists = false;
			using (var connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();
				string sqlSelect = string.Format("select 1 from dbo.PriceMarket where Url = '{0}'", url);
				SqlCommand command = new SqlCommand(sqlSelect, connection);
				SqlDataReader reader = command.ExecuteReader();

				if (reader.Read())
					exists = true;
			}

			return exists;
		}

		public void LogMessage(string message)
		{
			LogMessages += string.Format("{0}{1}", message, Environment.NewLine);
		}

		private Dictionary<int, Dictionary<int, string>> ReadExcel(string excelFilename, List<int> columnNumbers, bool readFirstLine)
		{
			var row2ColumnsValues = new Dictionary<int, Dictionary<int, string>>();
			Application excelApp = new Application();
			if (excelApp != null)
			{
				Workbook excelWorkbook = excelApp.Workbooks.Open(excelFilename, 0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
				Worksheet excelWorksheet = (Worksheet)excelWorkbook.Sheets[1];

				Range excelRange = excelWorksheet.UsedRange;
				int rowCount = excelRange.Rows.Count;
				int colCount = excelRange.Columns.Count;
				int startIndex = readFirstLine ? 1 : 2;

				for (int i = startIndex; i <= rowCount; i++)
				{
					var column2Value = new Dictionary<int, string>();
					foreach (var columnIndex in columnNumbers)
					{
						Range range = (excelWorksheet.Cells[i, columnIndex] as Range);
						string value = string.Empty;
						if (range.Value != null)
							value = range.Value.ToString().Trim();

						column2Value[columnIndex] = value;
					}

					row2ColumnsValues[i] = column2Value;
				}

				excelWorkbook.Close();
				excelApp.Quit();
			}

			return row2ColumnsValues;
		}

		//private void ReadPrice()
		//{
		//    foreach (var row2ColumnsValues in ReadExcel(SourcePrice, new List<int> { 1, 8 }, false).Values)
		//    {
		//        var code = row2ColumnsValues[1];
		//        var price = row2ColumnsValues[8];

		//        if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(price))
		//            Price.Add(new Price(code, Convert.ToDecimal(price)));
		//    }
		//}

		private List<string> ReadUrls()
		{
			List<string> urls = new List<string>();
			if (File.Exists(Urls2Update))
			{
				foreach (var row2ColumnsValues in ReadExcel(Urls2Update, new List<int> { 1 }, false).Values)
					urls.Add(row2ColumnsValues[1]);
			}

			return urls;
		}

		private void AddIncorectProduct(string productUl, string columnName, string oldValue, string newValue)
		{
			oldValue = oldValue.Replace("'", "''");
			if (oldValue.Length > 250)
				oldValue = oldValue.Substring(0, 250);

			newValue = newValue.Replace("'", "''");
			if (newValue.Length > 250)
				newValue = newValue.Substring(0, 250);

			using (var connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();

				string sqlUpdate = string.Format("insert into dbo.IncorrectProducts(Url, ColumnName, OldValue, NewValue, Updated) values ('{0}', '{1}', '{2}', '{3}', getdate())", productUl, columnName, oldValue, newValue);
				SqlCommand command = connection.CreateCommand();
				command.CommandText = sqlUpdate;
				command.ExecuteNonQuery();
			}
		}

		private int? GetLastModificationId()
		{
			using (var connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();
				string sqlSelect = "select MAX(id) from [dbo].[IncorrectProducts]";
				SqlCommand command = new SqlCommand(sqlSelect, connection);
				SqlDataReader reader = command.ExecuteReader();

				if (reader.Read())
				{
					if (string.IsNullOrEmpty(reader.GetValue(0).ToString()))
						return null;
					return Convert.ToInt32(reader.GetValue(0).ToString());
				}
			}

			return null;
		}

		private void UpdateProductPrice(SqlConnection connection, ProductInfo product)
		{
			string sqlUpdate = string.Empty;
			try
			{
				sqlUpdate = string.Format("update dbo.PriceMarket set Price = {0} where ProductCode = '{1}' and Price <> {0}", product.Price.ToString().Replace(",", "."), product.ProductCode);
				SqlCommand command = connection.CreateCommand();
				command.CommandText = sqlUpdate;
				command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				Logger.LogMessage(sqlUpdate);
				Logger.LogMessage(e.Message);
				Logger.LogMessage(e.StackTrace);
			}
		}

		private void UpdateProductPicture(SqlConnection connection, ProductInfo product)
		{
			//string sqlUpdate = string.Format("update dbo.PriceMarket set PictureUrl = '{0}', isPrice='0' where ProductCode = '{1}'", product.UrlPicture, product.ProductCode);
			string sqlUpdate = string.Format("update dbo.PriceMarket set PictureUrl = '{0}' where ProductCode = '{1}'", product.UrlPicture, product.ProductCode);
			SqlCommand command = connection.CreateCommand();
			command.CommandText = sqlUpdate;
			command.ExecuteNonQuery();
		}

		private void UpdateProductDescription(SqlConnection connection, ProductInfo product)
		{
			//string sqlUpdate = string.Format("update dbo.PriceMarket set Description = '{0}', isPrice='0' where ProductCode = '{1}'", product.Description.Replace("'", "''"), product.ProductCode);
			string sqlUpdate = string.Format("update dbo.PriceMarket set Description = '{0}' where ProductCode = '{1}'", product.Description.Replace("'", "''"), product.ProductCode);
			SqlCommand command = connection.CreateCommand();
			command.CommandText = sqlUpdate;
			command.ExecuteNonQuery();
		}

		private ProductInfo LoadProduct(string sqlSelect)
		{
			using (var connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();

				SqlCommand command = new SqlCommand(sqlSelect, connection);
				SqlDataReader reader = command.ExecuteReader();
				if (reader.Read())
					return Constants.FillProdactFromDB(reader);
			}

			return null;
		}

		private void ReloadAllProducts()
		{
			DeleteAllProducts();

			List<string> lines = new List<string>();

			//lines.Add(@"http://www.vofisplus.by/catalog/eid224677.html");
			//lines.Add(@"http://www.vofisplus.by/catalog/eid353549.html");
			//lines.Add(@"http://www.vofisplus.by/catalog/eid405988.html");

			CultureInfo[] cultures = { new CultureInfo("en-US"),
								 new CultureInfo("fr-FR") };

			if (!lines.Any())
				lines.AddRange(File.ReadAllLines(SourceTxtFile));

			using (var connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();

				foreach (var line in lines)
				{
					int index = line.IndexOf(@"/eid");
					if (index < 0)
						continue;
					index += 4;

					if (IsUrlExists(line))
						continue;

					ProductInfo product = new ProductInfo();
					product.Url = line;

					int idIndex = line.IndexOf(".html");

					try
					{
						product.Id = Convert.ToInt32(line.Substring(index, idIndex - index));
					}
					catch(Exception e)
					{
						Logger.LogMessage(string.Format("Can't get product Id: {0}", line));
						continue;
					}

					var pageInfo = GetWebPageInfo(product.Url);
					if (pageInfo == null
					|| string.IsNullOrEmpty(pageInfo.Price)
					|| string.IsNullOrEmpty(pageInfo.ImageBox))
						continue;

					bool converted = false;
					foreach (CultureInfo culture in cultures)
					{
						try
						{

							product.Price = Convert.ToDecimal(pageInfo.Price.Replace(",", "."), culture);
							converted = true;
							break;
						}
						catch (Exception)
						{
						}
					}

					if (!converted)
					{
						Logger.LogMessage(string.Format("Can't convert price from VofisPlus, price value: {0} page: {1}", pageInfo.Price, product.Url));
						continue;
					}

					//product.Price = Convert.ToDecimal(pageInfo.Price);
					//product.Price = Convert.ToDecimal(pageInfo.Price.Replace(",", "."));
					product.Description = pageInfo.Description;
					product.Category = pageInfo.ParentTitle;
					product.UrlPicture = pageInfo.ImageBox;
					product.Vendor = pageInfo.ParamsTableVendor;
					product.ProductCode = pageInfo.ParamsProductCode;

					pageInfo.Title = pageInfo.Title.Replace(" - vofisplus", "");
					pageInfo.Title = pageInfo.Title.Replace("&times;", " x ");

					int nameEndIndex = pageInfo.Title.IndexOf(',');
					product.Name = pageInfo.Title.Substring(0, nameEndIndex);
					product.Model = CorrectText(pageInfo.Title.Substring(nameEndIndex + 1, pageInfo.Title.Length - nameEndIndex - 1));

					//delete existed record before insert
					//DeleteProducts(connection, new List<string> { product.ProductCode });

					//sql
					string salInsert = String.Format("insert into dbo.PriceMarket(Id, Url, Price, Category, PictureUrl, Vendor, Model, Description, Name, ProductCode) values({0}, '{1}', {2}, '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')"
					, product.Id
					, product.Url
					, product.Price.ToString().Replace(",", ".")
					, product.Category.Replace("'", "''")
					, product.UrlPicture
					, product.Vendor.Replace("'", "''")
					, product.Model.Replace("'", "''")
					, product.Description.Replace("'", "''")
					, product.Name.Replace("'", "''")
					, product.ProductCode.Replace("'", "''")
					);
					SqlCommand command = connection.CreateCommand();
					command.CommandText = salInsert;
					command.ExecuteNonQuery();

					++NewRecords;
				}
			}
		}

		private static WebPageInfo GetWebPageInfo(string url)
		{
			try
			{
				WebPageContent = GetWebPageContent(url);
				WebPageInfo info = new WebPageInfo();
				info.Price = GetWebPagePrice();
				if (!string.IsNullOrEmpty(info.Price))
				{
					info.Title = GetWebPageTitle();
					info.Description = GetWebPageDescription();
					info.ParentTitle = GetWebPageParentTitle();
					info.ImageBox = GetWebPageImageBox();
					info.ParamsTableVendor = GetWebPageParamsTableVendor();
					info.ParamsProductCode = GetWebPageParamsTableProductCode().Trim();
				}
				return info;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string GetWebPageContent(string url, Encoding encoding = null)
		{
            if(encoding == null)
                encoding = Encoding.GetEncoding(1251);

            HttpWebRequest request = (HttpWebRequest.Create(url) as HttpWebRequest);
			HttpWebResponse response = (request.GetResponse() as HttpWebResponse);
			string contents = string.Empty;

			using (Stream stream = response.GetResponseStream())
			{
				int bytesToRead = 8092;
				byte[] buffer = new byte[bytesToRead];

				int length;
				while ((length = stream.Read(buffer, 0, bytesToRead)) > 0)
				{
					contents += encoding.GetString(buffer, 0, length);
				}
			}

			return contents;
		}

		private static string GetWebPageTitle()
		{
			return GetWebPageParameter(new Regex(@"<title>\s*(.+?)\s*</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase));
		}

		private static string GetWebPagePrice()
		{
			string regexString = "<span class='price nowrap'>_s*(.+?)_s*</span>";
			string replacePrefix = "<span itemprop='price'>";

			replacePrefix = replacePrefix.Replace("'", "\"");
			regexString = regexString.Replace("'", "\"");
			regexString = regexString.Replace("_", "\\");
			string result = GetWebPageParameter(new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase));
			if (string.IsNullOrEmpty(result))
				return string.Empty;

			result = result.Replace(replacePrefix, "");
			result = result.Replace("р.", "");
			return result.Replace(" ", "");
		}

		private static string GetWebPageDescription()
		{
			string priceWrap = @"""additionalDescription"" itemprop=""description""";
			string regexString = String.Format(@"<div class={0}>\s*(.+?)\s*</div>", priceWrap);
			return CorrectText(GetWebPageParameter(new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase)));
		}

		private static string CorrectText(string text)
		{
			if (text.Contains("<span class="))
			{
				string start = "\"";
				start += ">";
				int index1 = text.IndexOf(start);
				if (index1 >= 0)
				{
					index1 += start.Length;
					int index2 = text.IndexOf(@"</span>");
					if (index2 >= 0)
						text = text.Substring(index1, index2 - index1);
				}
			}

			if (text.Contains("href="))
			{
				int index = text.IndexOf("href=");
				int pointIndex = -1;

				for (int i = 0; i < index; i++)
				{
					if (text[i] == '.')
						pointIndex = i;
				}

				if (pointIndex == -1)
					return string.Empty;

				text = text.Substring(0, pointIndex);
			}

			text = text.Replace("<b>", "");
			text = text.Replace("</b>", "");
			text = text.Replace("<br />", "");
			text = text.Replace("&laquo;", "");
			text = text.Replace("&raquo;", "");
			text = text.Replace("&times;", " x ");
			text = text.Replace("&sup", "");
			text = text.Replace("&mdash;", "-");
			text = text.Replace("<nosearch>", "");
			text = text.Replace("</nosearch>", "");
			text = text.Replace("&deg;", "");
			text = text.Replace("</div>", "");
			text = text.Replace("<div>", "");
			text = text.Replace("&asymp;", "");
			text = text.Replace("<p>", "");
			text = text.Replace("</p>", "");
			text = text.Replace("<br/><br/>", "");
			text = text.Replace("<strong>", "");
			text = text.Replace("</strong>", "");
			text = text.Replace("</span>", "");
			text = text.Replace("<span>", "");
			text = text.Replace("<font color=\"#585858\" face=\"Arial\"><span style=\"font-size: 12px;\">.  </font>", " ");
			text = text.Replace("<font color=\"#4c4c4c\" face=\"Tahoma, Geneva, Arial, sans-serif\"><span style=\"font-size: 12px; line-height: 16px; background-color: rgb(255, 255, 255);\"> </font>", " ");
			text = text.Replace("<div style=\"margin-top: 8px !important;\">", "");
			text = text.Replace("<div style=\"text-align: justify;\">", "");
			text = text.Replace("<span style=\"text-align: justify;\">", "");
			//text = text.Replace("", "");
			//text = text.Replace("", "");

			return text;
		}

		private static string GetWebPageParentTitle()
		{
			string priceWrap = String.Format("\"parent_title\"");
			string regexString = String.Format(@"<a class={0} \s*(.+?)\s*</a>", priceWrap);
			string row = GetWebPageParameter(new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase));

			int index1 = row.IndexOf("\"row\">") + 6;
			int index2 = row.IndexOf("</span>");

			return row.Substring(index1, index2 - index1);
		}

		private static string GetWebPageImageBox()
		{
			string priceWrap = String.Format("\"img_box\"");
			string searchString = String.Format(@"<div class={0}>", priceWrap);
			string row = GetWebPageParameter2(searchString);

			if (string.IsNullOrEmpty(row))
				return string.Empty;

			int index1 = row.IndexOf("src=\"") + 6;
			int index2 = row.IndexOf("\" alt=");
			int len = index2 - index1;
			if (len <= 0)
				return string.Empty;

			string imageUrl = @"http://vofisplus.by/" + row.Substring(index1, len);

			return IsImageFileExists(imageUrl) ? imageUrl : string.Empty;
		}

		private static bool IsImageFileExists(string imageUrl)
		{
			var wreq = (HttpWebRequest)WebRequest.Create(imageUrl);

			//wreq.KeepAlive = true;
			wreq.Method = "HEAD";
			HttpWebResponse wresp = null;

			try
			{
				wresp = (HttpWebResponse)wreq.GetResponse();

				return (wresp.StatusCode == HttpStatusCode.OK);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(String.Format("url: {0} not found", imageUrl));
				return false;
			}
			finally
			{
				if (wresp != null)
				{
					wresp.Close();
				}
			}
		}

		private static string GetWebPageParamsTableVendor()
		{
			string priceWrap = String.Format("\"params_table\"");
			string searchString = String.Format(@"<table class={0}>", priceWrap);
			string row = GetWebPageParameter2(searchString);

			string tradeMark = "<td>Торговая марка</td>";
			int index = row.IndexOf(tradeMark) + tradeMark.Length;
			string row2 = row.Substring(index, row.Length - index);

			int index1 = row2.IndexOf("<td>") + 4;
			int index2 = row2.IndexOf("</td>");

			return row2.Substring(index1, index2 - index1);
		}

		private static string GetWebPageParamsTableProductCode()
		{
			string priceWrap = String.Format("\"params_table\"");
			string searchString = String.Format(@"<table class={0}>", priceWrap);
			string row = GetWebPageParameter2(searchString);

			string productCode = "<td>Код товара</td>";
			int index = row.IndexOf(productCode) + productCode.Length;
			string row2 = row.Substring(index, row.Length - index);

			int index1 = row2.IndexOf("<td>") + 4;
			int index2 = row2.IndexOf("</td>");

			return row2.Substring(index1, index2 - index1);
		}


		private static string GetWebPageParameter(Regex regex)
		{
			Match m = regex.Match(WebPageContent);
			if (m.Success)
				return m.Groups[1].Value.ToString();

			return string.Empty;

		}

		private static string GetWebPageParameter2(string searchString)
		{
			int index = WebPageContent.IndexOf(searchString);
			if (index >= 0)
				return WebPageContent.Substring(index, WebPageContent.Length - index);

			return string.Empty;
		}

		private void SetNewPrice(ProductInfo product)
		{
			using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();

				string sqlUpdate = string.Format("update dbo.PriceMarket set isNextPrice=1 where ProductCode = '{0}'", product.ProductCode);
				SqlCommand command = connection.CreateCommand();
				command.CommandText = sqlUpdate;
				command.ExecuteNonQuery();
			}
		}

		private void SyncPrice()
		{
			using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();

				string sqlUpdate = "update dbo.PriceMarket set isPrice=null";
				SqlCommand command = connection.CreateCommand();
				command.CommandText = sqlUpdate;
				command.ExecuteNonQuery();

				sqlUpdate = "update dbo.PriceMarket set isPrice=isNextPrice";
				command = connection.CreateCommand();
				command.CommandText = sqlUpdate;
				command.ExecuteNonQuery();

				sqlUpdate = "update dbo.PriceMarket set isNextPrice=null";
				command = connection.CreateCommand();
				command.CommandText = sqlUpdate;
				command.ExecuteNonQuery();
			}
		}

		private bool CheckProductAdditionalParameters(string productUrl, decimal oldProductPrice)
		{
			WebPageContent = GetWebPageContent(productUrl);
			string price = GetWebPagePrice();
			if (string.IsNullOrEmpty(price))
				return false;

			decimal newProductPrice = Convert.ToDecimal(price);
			if (oldProductPrice != newProductPrice)
				return false;

			if (string.IsNullOrEmpty(GetWebPageDescription()) || string.IsNullOrEmpty(GetWebPageImageBox()))
				return false;

			return true;
		}

		private void GenerateExcelPrice()
		{
			if (File.Exists(TargetPricePath))
				File.Delete(TargetPricePath);

			LastModificationId = GetLastModificationId();

			var rows = new List<List<string>>();
			using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();

				int maxCount = 100000;
				int index = 0;

				string sqlSelect = string.Format("{0} where price <= 2000000 and isnull(Description, '') <> '' and isnull(PictureUrl, '') <> '' and Url not in (SELECT distinct url FROM [Work].[dbo].[IncorrectProducts] where Updated > DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)) order by id", Constants.SqlSelectAllVofisPlusProducts);
				foreach (var product in Constants.LoadProducts(sqlSelect))
				{
					try
					{
						bool isPrice = true;
						//get info from page: Price
						WebPageContent = GetWebPageContent(product.Url);
						decimal price = 0;
						string webPrice = GetWebPagePrice();
						if (!string.IsNullOrEmpty(webPrice))
						{
							price = Convert.ToDecimal(webPrice);

						}
						else
							isPrice = false;

						if (product.Price != price)
						{
							AddIncorectProduct(product.Url, "Price", product.Price.ToString(), price.ToString());
							product.Price = price;
							UpdateProductPrice(connection, product);
							++UpdatedPriceFields;
							//isPrice = false;
						}

						//get info from page: Picture
						string webPicture = GetWebPageImageBox();
						if (product.UrlPicture != webPicture)
						{
							AddIncorectProduct(product.Url, "Picture", product.UrlPicture, webPicture);
							product.UrlPicture = webPicture;
							UpdateProductPicture(connection, product);
							++UpdatedPriceFields;
							//isPrice = false;
						}

						//get info from page: Description
						string webDescription = GetWebPageDescription();
						if (product.Description != webDescription)
						{
							AddIncorectProduct(product.Url, "Description", product.Description, webDescription);
							product.Description = webDescription;
							UpdateProductDescription(connection, product);
							++UpdatedPriceFields;
							//isPrice = false;
						}

						if (isPrice && index < maxCount)
						{
							List<string> row = new List<string>();
							row.Add(product.Id.ToString()); //id
							row.Add("vendor.model"); //type
							row.Add("true"); //available
							row.Add(""); //bid
							row.Add(product.Url); //url
							row.Add(product.Price.ToString()); //price
							row.Add("BYN"); //currencyId
							row.Add(product.Category); //category
							row.Add(product.UrlPicture); //picture
							row.Add("true"); //delivery
							row.Add(""); //local_delivery_cost
							row.Add(""); //typePrefix
							row.Add(product.Name); //vendor !!!
							row.Add(product.Model); //model
							row.Add(product.Description); //description
							row.Add(product.DealKeywords); //description
							rows.Add(row);

							SetNewPrice(product);
							++index;
						}
					}
					catch (Exception e)
					{
						string msg = string.Format("Error: {0}; stack: {1}; product: {2}", e.Message, e.StackTrace, product.ToString());
						Logger.LogMessage(msg);
					}
				}
			}

			//var reportSheet = new ExcelReportSheet();
			//reportSheet.SheetName = "Sheet1";
			//reportSheet.HeaderNames = GetHeaders();
			//reportSheet.ReportData = rows;

			//var reportSheets = new List<ExcelReportSheet>();
			//reportSheets.Add(reportSheet);

			List<ExcelData> sheets = new List<ExcelData>();
			ExcelData productReportSheet = new ExcelData();
			productReportSheet.SheetName = "Sheet1";
			productReportSheet.Headers = GetHeaders();
			productReportSheet.DataRows = rows;
			sheets.Add(productReportSheet);

			//var helper = new ExcelHelper();
			//helper.CreateExcel(TargetPricePath, reportSheets);

			var excelGenerator = new ExcelGenerator();
			excelGenerator.GenerateExcel(sheets, TargetPricePath);
		}

		private void DeleteAllProducts()
		{
			using (var connection = new SqlConnection(Constants.ConnectionString))
			{
				connection.Open();

				string sqlDelete = "delete from dbo.PriceMarket";
				SqlCommand command = connection.CreateCommand();
				command.CommandText = sqlDelete;
				command.ExecuteNonQuery();

				sqlDelete = "delete from dbo.IncorrectProducts";
				SqlCommand command2 = connection.CreateCommand();
				command2.CommandText = sqlDelete;
				command2.ExecuteNonQuery();
			}
		}

		private void DeleteProducts(SqlConnection connection, List<string> productCodes)
		{
			foreach (var code in productCodes)
			{
				string sqlDelete = string.Format("delete from dbo.PriceMarket where ProductCode = '{0}'", code);
				SqlCommand command = connection.CreateCommand();
				command.CommandText = sqlDelete;
				command.ExecuteNonQuery();

				++DeletedRecords;
			}
		}

		public List<string> GetHeaders()
		{
			var headers = new List<string>();
			headers.Add("id");
			headers.Add("type");
			headers.Add("available");
			headers.Add("bid");
			headers.Add("url");
			headers.Add("price");
			headers.Add("currencyId");
			headers.Add("category");
			headers.Add("picture");
			headers.Add("delivery");
			headers.Add("local_delivery_cost");
			headers.Add("typePrefix");
			headers.Add("vendor");
			headers.Add("model");
			headers.Add("description");
			return headers;
		}
	}
}
