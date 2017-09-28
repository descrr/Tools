using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Globalization;

namespace PriceGenerator
{
    public class PriceAnalyzer
    {
        private List<CompetitorArticle> CompetitorArticles = new List<CompetitorArticle>();
        private List<BasePriceChecker> PriceCheckers = new List<BasePriceChecker>();

        public PriceAnalyzer()
        {
            //deal
            // AddPriceAnalizer(@"<div class='b-product__price-holder' itemprop='offers' itemscope='itemscope' itemtype='http://schema.org/Offer'><p class='b-product__price'><span itemprop='price' content='", '"');
            
            //chay.by
            //AddPriceAnalizer(@"<span class='pricet'>", 'С', false);
            AddPriceAnalizer(@"<span class='pricet'>", '<', false);

            //blackstore.by
            //AddPriceAnalizer(@"<meta itemprop='price' content='", 'B');

            //officeton.by
            AddPriceAnalizer(@"Цена без НДС: <strong>", '<', false);

        }

        private void AddPriceAnalizer(string prefix, char delimiter, bool replace = true)
        {
            if(replace)
                prefix = prefix.Replace("'", "\"");
            PriceCheckers.Add(new BasePriceChecker(prefix, delimiter));
        }

        public void AnalysePrice()
        {
            try
            {
                LoadCompetitors();
                CheckPrice();
            }
            catch (Exception e)
            {
                Logger.LogMessage(e.Message);
                Logger.LogMessage(e.StackTrace);
            }
        }

        private void CheckPrice()
        {
            foreach (var competitorArticle in CompetitorArticles)
            {
                var myPrice = GetArticlePrice(competitorArticle.ManufacturerArticle);
                if (myPrice == null)
                    continue;

                var minCompetitorsVatPrice = GetMinCompetitorVatPrice(competitorArticle, myPrice.VatRate);
                decimal myMinVatPrice = Math.Round(myPrice.SupplierCost * (decimal)1.25 * ((decimal)1 + myPrice.VatRate/(decimal)100), 2);
                var newVatPrice = (minCompetitorsVatPrice - minCompetitorsVatPrice / (decimal)100);
                newVatPrice = Math.Round(newVatPrice, 2);
                //if (myPrice.VatPrice != newVatPrice)
                //{
                //    if (newVatPrice > myMinVatPrice)
                //        SetNewPrice(competitorArticle.ManufacturerArticle, newNotVatPrice);
                //    else
                //    {
                //        try
                //        {
                //            string msg = string.Format("У конкурентов лучшая цена по товару http://www.vofis.by/p/{0}.aspx", myPrice.ProductId);
                //            var sender = new MailSender("descrr@gmail.com", "vh@vofis.by", "gva212gva212!", msg);
                //            sender.SendMail(msg, new List<string>());
                //        }
                //        catch(Exception)
                //        { }
                //    }
                //}
            }
        }

        private void SetNewPrice(string manufacturerArticle, decimal price)
        {
            int productId = GetProductId(manufacturerArticle);
            if (productId == -1)
                return;

            decimal vat = GetProductVAT(manufacturerArticle);
            if (vat > -1)
            {
                decimal factor = (decimal) 1 + vat/(decimal)100;
                decimal newPrice = price/factor;
                UpdatePrice(productId,  Decimal.Round(newPrice, 2).ToString("#0.00#").Replace(",","."));
            }
        }

        private void UpdatePrice(int productId, string price)
        {
            InsertPriceLine(productId);
            ResetCostOnOff(productId);
            UpdatePriceLine(productId, price);
        }

        private void UpdatePriceLine(int productId, string price)
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                string sqlUpdate = string.Format("update [GF_Vofis].[dbo].[T_ProductCost]  set [OnOffer] = 1, CreatedTime = GetDate(), Cost = {1}  where ProductCostID = (select max(ProductCostID) from [GF_Vofis].[dbo].[T_ProductCost] where ProductID = {0})", productId, price);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = sqlUpdate;
                command.ExecuteNonQuery();
            }
        }

        private void InsertPriceLine(int productId)
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                string sqlUpdate = string.Format(@" insert into [GF_Vofis].[dbo].[T_ProductCost]([ProductID]
                                                  ,[StoreID]
                                                  ,[Cost]
                                                  ,[Quantity]
                                                  ,[OnOffer]
                                                  ,[Blocked]
                                                  ,[ProductCostHistoryID]
                                                  ,[SupplierCost]
                                                  ,[VATRate]
                                                  ,[CreatedTime]
                                                  ,[SellMultiple]
                                                  ,[QuantityInCargoPlace]
                                                  ,[ManufacturerArticle])
select [ProductID]
                                                  ,[StoreID]
                                                  ,[Cost]
                                                  ,[Quantity]
                                                  ,[OnOffer]
                                                  ,[Blocked]
                                                  ,[ProductCostHistoryID]
                                                  ,[SupplierCost]
                                                  ,[VATRate]
                                                  ,[CreatedTime]
                                                  ,[SellMultiple]
                                                  ,[QuantityInCargoPlace]
                                                  ,[ManufacturerArticle]
from [GF_Vofis].[dbo].[T_ProductCost]
where [ProductCostID] = (select max(ProductCostID) from [GF_Vofis].[dbo].[T_ProductCost] where ProductID = {0})", productId);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = sqlUpdate;
                command.ExecuteNonQuery();
            }
        }

        private void ResetCostOnOff(int productId)
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                string sqlUpdate = string.Format("update [GF_Vofis].[dbo].[T_ProductCost]  set [OnOffer] = 0 where ProductID = '{0}'", productId);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = sqlUpdate;
                command.ExecuteNonQuery();
            }
        }

        private int GetProductId(string manufacturerArticle)
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                string sqlSelect = string.Format("select productId FROM [GF_Vofis].[dbo].[T_ProductCost] where ManufacturerArticle = '{0}'", manufacturerArticle);
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                    return Convert.ToInt32(reader.GetValue(0).ToString());
                return -1;
            }
        }

        private decimal GetProductVAT(string manufacturerArticle)
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                string sqlSelect = string.Format("select vatrate FROM [GF_Vofis].[dbo].[T_ProductCost] where ManufacturerArticle = '{0}'", manufacturerArticle);
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                    return Convert.ToDecimal(reader.GetValue(0).ToString());
                return -1;
            }
        }
        
        public class ArticlePrice
        {
            public decimal VatPrice { get; set; }
            public int Natsenka { get; set; }

            public decimal VatRate { get; set; }

            public decimal SupplierCost { get; set; }

            public int ProductId { get; set; }
        }
        private ArticlePrice GetArticlePrice(string manufacturerArticle)
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                string sqlSelect = string.Format("select cost + cost/100*vatrate, cast(round((cost/supplierCost - 1)*1000, -1)/10 as int), vatrate, SupplierCost, ProductId FROM [GF_Vofis].[dbo].[T_ProductCost] where ManufacturerArticle = '{0}' and OnOffer=1", manufacturerArticle);
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var articlePrice = new ArticlePrice();
                    articlePrice.VatPrice = Math.Round(Convert.ToDecimal(reader.GetValue(0).ToString()), 2);
                    articlePrice.Natsenka = Convert.ToInt32(reader.GetValue(1).ToString());
                    articlePrice.VatRate = Convert.ToDecimal(reader.GetValue(2).ToString());
                    articlePrice.SupplierCost = Convert.ToDecimal(reader.GetValue(3).ToString());
                    articlePrice.ProductId = Convert.ToInt32(reader.GetValue(4).ToString());
                    return articlePrice;                    
                }
                return null;
            }
        }

        private Encoding GetEncoding(string competitorEncoding)
        {
            if (competitorEncoding == "utf-8")
                return Encoding.UTF8;

            return null;
        }

        private decimal GetMinCompetitorVatPrice(CompetitorArticle competitorArticle, decimal vatRate)
        {
            decimal minCompetitorPrice = -1;
            foreach (var competitorData in competitorArticle.CompetitorLinksList)
            {
                var competitorPrice = GetCompetitorVatPrice(competitorData.Link, GetEncoding(competitorData.Encoding), competitorData.IsVatPrice, vatRate);
                if (competitorPrice > -1
                    && (minCompetitorPrice == -1 || minCompetitorPrice > competitorPrice))
                    minCompetitorPrice = competitorPrice;
            }
            return minCompetitorPrice;
        }

        private decimal GetCompetitorVatPrice(string url, Encoding encoding, bool isVatPrice, decimal vatRate)
        {
            try
            {
                var page = ProductCollector.GetWebPageContent(url, encoding);
                var price =  GetPriceFromPage(page);
                if(!isVatPrice)
                {
                    price = price * ((decimal)1 + vatRate / (decimal)100);
                }

                return Math.Round(price, 2);
            }
            catch (Exception e)
            {
                Logger.LogMessage("Warning: " + e.Message);
                return -1;
            }
        }

        private decimal GetPriceFromPage(string page)
        {
            foreach(var priceChecker in PriceCheckers)
            {
                decimal price = -1;
                try
                {
                    price = priceChecker.GetPrice(page);
                    if (price > -1)
                        return price;
                }
                catch(Exception e)
                {
                    Logger.LogMessage("Warning: " + e.Message);
                    continue;
                }
            }
            return -1;
        }

        private void LoadCompetitors()
        {
            using (var connection = new SqlConnection(Constants.ConnectionString))
            {
                connection.Open();
                string sqlSelect = "select ManufacturerArticle, copmetitorLink, copmpetitorEncoding, [PriceWithVat] from dbo.CompetitorsLinks";
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string article = reader.GetValue(0).ToString();
                    string url = reader.GetValue(1).ToString();
                    string encoding = reader.GetValue(2).ToString();
                    bool isVatPrice = reader.GetValue(3).ToString()=="True";

                    var foundArticle = CompetitorArticles.Where(p => p.ManufacturerArticle == article).FirstOrDefault();
                    if (foundArticle == null)
                    {
                        foundArticle = new CompetitorArticle();
                        foundArticle.ManufacturerArticle = article;
                        foundArticle.CompetitorLinksList = new List<CompetitorData>();
                    }
                    foundArticle.CompetitorLinksList.Add(new CompetitorData { Link = url, Encoding = encoding, IsVatPrice = isVatPrice });
                    CompetitorArticles.Add(foundArticle);
                }
            }
        }
}
    
    public class CompetitorData
    {
        public string Link;
        public string Encoding;
        public bool IsVatPrice;
    }
    public class CompetitorArticle
    {
        public string ManufacturerArticle { get; set; }

        public List<CompetitorData> CompetitorLinksList { get; set; }
    }

    public class BasePriceChecker
    {
        protected string SearchPattern;
        protected string Delimiter;

        public BasePriceChecker(string searchPattern, char delimiter)
        {
            SearchPattern = searchPattern;
            Delimiter = delimiter.ToString();
        }

		public BasePriceChecker(string searchPattern, string delimiter)
		{
			SearchPattern = searchPattern;
			Delimiter = delimiter;
		}

		public string GetElementValue(string page)
		{
			//prefix = prefix.Replace("'", "\"");

			string prefix = SearchPattern;

			int startIndex = page.IndexOf(prefix);
			if (startIndex < 0)
				return string.Empty;

			string newPage = page.Substring(startIndex);
			startIndex += prefix.Length;

			int endIndex = newPage.IndexOf(Delimiter);
			if (endIndex < 0)
				return string.Empty;

			return newPage.Substring(prefix.Length, endIndex - prefix.Length);
			// Convert.ToDecimal(price, new CultureInfo("en-US"));
		}

		public virtual decimal GetPrice(string page)
        {
			string price = GetElementValue(page);
			return Convert.ToDecimal(price, new CultureInfo("en-US"));
        }
	}

    //public class DealPriceChecker : BasePriceChecker
    //{
    //    public DealPriceChecker():base("","")
    //    {

    //    }

    //    public decimal GetPrice(string page);
    //}
}
