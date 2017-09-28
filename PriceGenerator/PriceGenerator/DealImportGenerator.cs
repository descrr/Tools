using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Luxottica.AdamExtensions.Maintenance.Jobs.DownloadViaFileConfiguration.Excel;
using Luxottica.AdamExtensions.Maintenance.Jobs.DownloadViaFileConfiguration.Helpers;

namespace PriceGenerator
{
    public class DealImportGenerator
    {
        private const string TargetPricePath = @"C:\home\vofisby\shared\vofis.deal.by.xml";
		private const string TargetYandexPricePath = @"C:\home\vofisby\shared\yandex.vofis.deal.by.xml";
        private const string TargetNgpFullPricePath = @"C:\home\vofisby\shared\ngp_caramella_full.csv";
        private const string TargetNgpPricePath = @"C:\home\vofisby\shared\ngp_caramella.csv";

        private string TmpPricePath;

        private CaramellaLinksGenerator CaramellaLinks = new CaramellaLinksGenerator();

        public DealImportGenerator()
        {
            TmpPricePath = string.Format(@"{0}\vofis.deal.by.xml", Directory.GetCurrentDirectory());
        }

        private List<string> GetProductHeaders()
        {
            List<string> ExcelColumnsNames = new List<string>();
            ExcelColumnsNames.Add("Название_позиции");
            ExcelColumnsNames.Add("Ключевые_слова");
            ExcelColumnsNames.Add("Описание");
            ExcelColumnsNames.Add("Тип_товара");
            ExcelColumnsNames.Add("Цена");
            ExcelColumnsNames.Add("Валюта");
            ExcelColumnsNames.Add("Скидка");
            ExcelColumnsNames.Add("Единица_измерения");
            ExcelColumnsNames.Add("Ссылка_изображения");
            ExcelColumnsNames.Add("Наличие");
            ExcelColumnsNames.Add("Идентификатор_товара");
            ExcelColumnsNames.Add("Идентификатор_группы");
            ExcelColumnsNames.Add("Оптовая_цена");
            ExcelColumnsNames.Add("Единица_измерения");
            ExcelColumnsNames.Add("Минимальный_заказ_опт");
            //ExcelColumnsNames.Add("Уникальный_идентификатор");

            return ExcelColumnsNames;
        }

        private List<string> GetFolderHeaders()
        {
            List<string> ExcelColumnsNames = new List<string>();
            ExcelColumnsNames.Add("Номер_группы");
            ExcelColumnsNames.Add("Название_группы");
            ExcelColumnsNames.Add("Идентификатор_группы");
            ExcelColumnsNames.Add("Номер_родителя");
            ExcelColumnsNames.Add("Идентификатор_родителя");

            return ExcelColumnsNames;
        }

        private string GetProductDescription(ProductInfo product)
        {
            string description = string.Format(@"{0}{1}{1} <p>&nbsp;</p> <div class='b-content__body b-user-content' itemprop='description'> <a href='http://deal.by/redirect?url={2}' target='_blank'><img alt='' src='http://vofis.by/shared/zakaz.jpg' style='width:265px;height:67px'></a> </div>", product.Description, Environment.NewLine, product.Url);
            
            //Caramella
            string caramellaAdditional = @"<p>&nbsp;</p><p>&nbsp;</p><h2><span style='color: rgb(192, 80, 77);'>Предлагаем вашему вниманию сайт новогодних подарков для детей
<a href='" + CaramellaLinks.GetLink() + @"'>Карамелла бай</a>
</span>
</h2>";

            description += caramellaAdditional;
            return description.Replace("'", "\"");
        }

        //private void SetDeal(ProductInfo product)
        //{
        //    using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
        //    {
        //        connection.Open();

        //        string sqlUpdate = string.Format("update dbo.PriceMarket set Deal = '1' where id = {0}", product.Id);
        //        SqlCommand commandInsert = connection.CreateCommand();
        //        commandInsert.CommandText = sqlUpdate;
        //        commandInsert.ExecuteNonQuery();
        //    }
        //}

        //private void ResetDeal()
        //{
        //    using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
        //    {
        //        connection.Open();

        //        string sqlUpdate = "update dbo.PriceMarket set Deal = null";
        //        SqlCommand commandInsert = connection.CreateCommand();
        //        commandInsert.CommandText = sqlUpdate;
        //        commandInsert.ExecuteNonQuery();
        //    }
        //}

        private List<List<string>> GetProductDataRows()
        {
            var productDataRows = new List<List<string>>();
            using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();

                string sqlSelect = string.Format("{0} where isnull([PictureUrl], '') <> '' and isnull([Description], '') <> '' and Price > 0 and Model not like '%акция.%' union {1}", Constants.SqlSelectAllVofisPlusProducts, Constants.SqlSelectAllVofisProducts);

                int maxCount = 50000;
                int i = 0;
                foreach (var product in Constants.LoadProducts(sqlSelect))
                {
                    string price = string.Empty;
                    if (product.Price != null)
                        price = product.Price.ToString();

                    List<string> row = new List<string>();
                    string productName = product.Name;
                    if (!string.IsNullOrEmpty(product.Model))
                        productName += String.Format(", {0}", product.Model);
                    if (!string.IsNullOrEmpty(product.Vendor))
                        productName += String.Format(", {0}", product.Vendor);
                    if (!string.IsNullOrEmpty(product.ProductCode))
                        productName += String.Format(". Код продукта: {0}", product.ProductCode);
                    productName = NormalizeProductName(productName);

                    row.Add(productName); //Название_позиции 0
                    row.Add(""); //Ключевые_слова   1
                    row.Add(GetProductDescription(product)); //Описание 2
                    row.Add("u"); //Тип_товара  3
                    row.Add(price); //Цена  4
                    row.Add("BYN"); //Валюта    5
                    row.Add(""); //Скидка   6
                    row.Add(""); //Единица_измерения    7
                    row.Add(product.UrlPicture); //Ссылка_изображения   8
                    row.Add("+"); //Наличие 9
                    row.Add(product.Id.ToString()); //Идентификатор_товара  10
                    row.Add(GetFolderId(product.Category)); //Идентификатор_группы  11
                    row.Add(product.Price.ToString()); //Оптовая_цена   12
                    row.Add("шт."); // Единица_измерения    13
                    row.Add("2"); // Минимальный_заказ_опт  14
                    row.Add(product.DealKeywords);//Deal keywords 15

                    productDataRows.Add(row);
                    //SetDeal(product);

                    ++i;
                    if (i >= maxCount)
                        break;
                }
            }
            return productDataRows;
        }

        private string NormalizeProductName(string productName)
        {
            productName = productName.Replace(Environment.NewLine, " ");
            productName = productName.Replace(@"\n", " ");
            string newName = productName.Replace("  ", " ");
            
            while (productName != newName)
            {
                productName = newName;
                newName = newName.Replace("  ", " ");
            }

            return productName;

        }
        private void AddNewFolders()
        {
            using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();

                string sqlSelect = String.Format("select distinct [Category] from [dbo].[PriceMarket] union select distinct Category from ( {0} ) t ", Constants.SqlSelectAllVofisProducts);
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                var productFolders = new List<string>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        productFolders.Add(reader.GetValue(0).ToString());
                }

                foreach (var folder in productFolders)
                {
                    string sqlInsert = string.Format("insert into [dbo].[ProductFolder](Id, FolderName, ParentId)   select (select max(id)+1 from [dbo].[ProductFolder]), '{0}', 646664   where not exists (select 1 from [dbo].[ProductFolder] where FolderName = '{0}')", folder);
                    SqlCommand commandInsert = connection.CreateCommand();
                    commandInsert.CommandText = sqlInsert;
                    commandInsert.ExecuteNonQuery();
                }
            }
        }

        private string GetFolderId(string folderName)
        {
            using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();

                string sqlSelect = string.Format("select id from [dbo].[ProductFolder] where FolderName = '{0}'", folderName);

                SqlCommand command = new SqlCommand(sqlSelect, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return reader.GetValue(0).ToString();

                return "646664";
            }
        }

        private List<List<string>> GetFolderDataRows()
        {
            AddNewFolders();

            var folderDataRows = new List<List<string>>();
            using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();

                foreach (var productFolder in Constants.LoadProductFolders(String.Format("{0} union {1}", Constants.SqlSelectAllVofisPlusProductFolders, Constants.SqlSelectAllVofisProductFolders)))
                {
                    List<string> row = new List<string>();
                    row.Add(productFolder.Id.ToString()); //Номер_группы    0
                    row.Add(productFolder.FolderName); //Название_группы    1
                    row.Add(productFolder.Id.ToString()); //Идентификатор_группы    2
                    row.Add(productFolder.ParentId.ToString()); //Номер_родителя    3
                    row.Add(productFolder.ParentId.ToString()); //Идентификатор_родителя    4

                    folderDataRows.Add(row);
                }
            }
            return folderDataRows;
        }

        public void GeneratePrice()
        {
            GenerateExcelPrice();
            GenerateCsvPrices();
        }

        private void GenerateExcelPrice()
        {
            if (File.Exists(TmpPricePath))
                File.Delete(TmpPricePath);
            YMLGenerator generator = new YMLGenerator();
            generator.Generate(GetProductDataRows(), GetFolderDataRows(), TmpPricePath);

            if (File.Exists(TargetPricePath))
                File.Delete(TargetPricePath);
            File.Move(TmpPricePath, TargetPricePath);
            GrantAccess(TargetPricePath);
        }

        private void GenerateCsvPrices()
        {
            if (File.Exists(TargetNgpFullPricePath))
                File.Delete(TargetNgpFullPricePath);
            if (File.Exists(TargetNgpPricePath))
                File.Delete(TargetNgpPricePath);

            var builderFull = new StringBuilder();
            var builder = new StringBuilder();

            builderFull.AppendLine(@"Product code;Language;Product id;Category;Price;Detailed image;Product name;Description");
            builder.AppendLine(@"Product code;Language;Product id;Category;Price;Detailed image");

            using (SqlConnection connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();

                string sqlSelect = @"SELECT p.[ProductID],p.[ProductName], p.Description, round(c.Cost*1.2, 2)
FROM [GF_Vofis].[dbo].[T_Product] p
inner join [GF_Vofis].[dbo].[T_ProductCost] c on p.ProductID = c.ProductID and c.onoffer = 1
where p.ProductFolderID = 318";

                SqlCommand command = new SqlCommand(sqlSelect, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string productPrice = reader.GetValue(3).ToString().Replace(",", ".");
                        string productId = reader.GetValue(0).ToString();
                        string productName = reader.GetValue(1).ToString();
                        string productDescription = reader.GetValue(2).ToString();
                        productDescription = productDescription.Replace("\"", "'");
                        productDescription = productDescription.Replace("«", "'");
                        productDescription = productDescription.Replace("»", "'");
                        productDescription = productDescription.Replace(Environment.NewLine, " ");
                        productDescription = productDescription.Replace("\n", " ");

                        builderFull.AppendLine(string.Format(@"""{0}"";""ru"";""{0}"";""Новогодние подарки///Коммунарка"";""{3}"";""http://www.vofis.by/upload/Product/Images/Medium/{0}.jpg"";""{1}"";""{2}""", productId, productName, productDescription, productPrice));
                        builder.AppendLine(string.Format(@"""{0}"";""ru"";""{0}"";""Новогодние подарки///Коммунарка"";""{1}"";""http://www.vofis.by/upload/Product/Images/Medium/{0}.jpg""", productId, productPrice));
                    }
                }
            }

            File.WriteAllText(TargetNgpFullPricePath, builderFull.ToString());
            File.WriteAllText(TargetNgpPricePath, builder.ToString());

            //GrantAccess(TargetNgpFullPricePath);
            //GrantAccess(TargetNgpPricePath);
        }
        
        private void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl,
                                                             InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                                                             PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }
    }
}
