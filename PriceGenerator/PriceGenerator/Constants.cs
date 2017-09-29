using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PriceGenerator
{
	public class Constants
	{
		public const string ConnectionStringTemplate = @"Data Source={0};Initial Catalog=Uno;Integrated Security=SSPI;";
		//

		public static string ConnectionString 
		{ 
			get
			{
				string dataSource = "GVAPC";
				if (Environment.MachineName == "HOMEPC")
					dataSource = @"HOMEPC\SQL2014";
				else if (Environment.MachineName == "EPBYMINW1589")
					dataSource = "EPBYMINW1589";
				return string.Format(ConnectionStringTemplate, dataSource);
			}
		}

		private const string SqlSelectAllAccounts = @"SELECT [id]
		  ,[name]
		  ,[insert_date]
		  ,[factor_vosst]
		  ,[nish_risk]
		  ,[prib_vol]
		  ,[sred_dnev_ub]
		  ,[kalmar]
		  ,[sharp]
		  ,[sortino]
		  ,[avg_doh_week]
		  ,[avg_doh_day]
	  FROM [dbo].[v_accounts]";

        public const string SqlSelectAllVofisPlusProducts = "SELECT Id, Url, Price, Category, PictureUrl, Vendor, Model, Description, Name, ProductCode, isPrice, dbo.[fn_GetProductDealKeywords](id) as DealKeywords FROM dbo.PriceMarket";
        public const string SqlSelectAllVofisProducts = @"select distinct 1000000+p.ProductID as Id, 'http://www.vofis.by/p/' + convert(varchar(10), p.ProductID) + '.aspx' as Url, cast (c.Cost + c.Cost/100*c.VATRate as decimal(6,2)) as Price, dbo.GetFolderName(f.ProductFolderName) as Category, 
(select top 1 replace(r.rsrURL, '~/', 'http://www.vofis.by/') as PictureUrl from GF_Vofis.dbo.T_Resource r where r.rsrAttrs = 'Original' and r.rsrEntityId = p.ProductID) as PictureUrl,
'' as Vendor, '' as Model, case when isnull(p.Description, '') = '' then p.ProductName else p.Description end as Description, p.ProductName as Name, '' as ProductCode, null as isPrice, GF_Vofis.dbo.[fn_GetProductDealKeywords](p.[ProductID]) as DealKeywords
 FROM GF_Vofis.dbo.T_Product p 
inner join  GF_Vofis.dbo.T_ProductCost c on p.ProductID = c.ProductID 
inner join  GF_Vofis.dbo.T_ProductFolder f on f.[ProductFolderID] = p.ProductFolderID
inner join  GF_Vofis.dbo.T_Resource r on r.rsrEntityId = p.ProductID and r.rsrAttrs = 'Original'
where OnOffer = 1
and [prfIsHidden] = 0
and isnull(r.rsrURL, '') <> '' and c.Cost > 0 and (select count(*) from GF_Vofis.dbo.T_ProductCost where onoffer=1 and ProductID = p.ProductID) = 1 ";

        public const string SqlSelectAllVofisPlusProductFolders = "select id, FolderName, ParentId from [dbo].[ProductFolder]";
        public const string SqlSelectAllVofisProductFolders = "select 1000000+ProductFolderID, dbo.GetFolderName(ProductFolderName), case when ParentProductFolderID is not null then case when ParentProductFolderID=1 then 646664 else 1000000+ParentProductFolderID end else 646664 end  from GF_Vofis.dbo.T_ProductFolder where prfIsHidden = 0 and IsVirtual = 0 and dbo.GetFolderName(ProductFolderName) not in('vofisplus.by', 'КАНЦТОВАРЫ')";

        public static  List<ProductInfo> LoadProducts(string sqlSelect)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var products = new List<ProductInfo>();
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    products.Add(FillProdactFromDB(reader));

                return products;
            }
        }

		public static List<Account> LoadAccounts()
		{
			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();

				var accounts = new List<Account>();
				SqlCommand command = new SqlCommand(SqlSelectAllAccounts, connection);
				SqlDataReader reader = command.ExecuteReader();
				while (reader.Read())
					accounts.Add(FillAccountFromDB(reader));

				return accounts;
			}
		}

		public static void UpdateAccountHistory(Account account)
		{
			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				
				string sqlInsert = string.Format("insert into dbo.account_history(account_id, factor_vosst, nish_risk, prib_vol, sred_dnev_ub, kalmar, sortino, sharp, avg_doh_week, avg_doh_day) values('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})"
					, account.Id
					, (account.FactorVosst == null)? "null": account.FactorVosst.ToString()
					, (account.NishRisk == null) ? "null" : account.NishRisk.ToString()
					, (account.PribVol == null) ? "null" : account.PribVol.ToString()
					, (account.SredDnevUb == null) ? "null" : account.SredDnevUb.ToString()
					, (account.Kalmar == null) ? "null" : account.Kalmar.ToString()
					, (account.Sortino == null) ? "null" : account.Sortino.ToString()
					, (account.Sharp == null) ? "null" : account.Sharp.ToString()
					, (account.DohWeek == null) ? "null" : account.DohWeek.ToString()
					, (account.DohDay == null) ? "null" : account.DohDay.ToString()
					);
				SqlCommand command = connection.CreateCommand();
				command.CommandText = sqlInsert;
				command.ExecuteNonQuery();
			}
		}

		public static List<ProductFolder> LoadProductFolders(string sqlSelect)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var productFolders = new List<ProductFolder>();
                SqlCommand command = new SqlCommand(sqlSelect, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    productFolders.Add(FillProdactFoldersFromDB(reader));

                return productFolders;
            }
        }
		
		public static Account FillAccountFromDB(SqlDataReader reader)
		{
			var accountId = reader.GetValue(0).ToString();
			var accountName = reader.GetValue(1).ToString();
			//date
			var accountFactorVosst = reader.GetValue(3).ToString();
			var accountNishRisk = reader.GetValue(4).ToString();
			var accountPribVol = reader.GetValue(5).ToString();
			var accountSredDnevUb = reader.GetValue(6).ToString();
			var accountKalmar = reader.GetValue(7).ToString();
			var accountSharp = reader.GetValue(8).ToString();
			var accountSortino = reader.GetValue(9).ToString();
			var accountAvgDohWeek = reader.GetValue(10).ToString();
			var accountAvgDohDay = reader.GetValue(11).ToString();

			var account = new Account()
			{
				Id = accountId,
				Name = accountName,
				FactorVosst = string.IsNullOrEmpty(accountFactorVosst) ? null : (decimal?)Convert.ToDecimal(accountFactorVosst),
				NishRisk = string.IsNullOrEmpty(accountNishRisk) ? null : (decimal?)Convert.ToDecimal(accountNishRisk),
				PribVol = string.IsNullOrEmpty(accountPribVol) ? null : (decimal?)Convert.ToDecimal(accountPribVol),
				SredDnevUb = string.IsNullOrEmpty(accountSredDnevUb) ? null : (decimal?)Convert.ToDecimal(accountSredDnevUb),
				Kalmar = string.IsNullOrEmpty(accountKalmar) ? null : (decimal?)Convert.ToDecimal(accountKalmar),
				Sharp = string.IsNullOrEmpty(accountSharp) ? null : (decimal?)Convert.ToDecimal(accountSharp),
				Sortino = string.IsNullOrEmpty(accountSortino) ? null : (decimal?)Convert.ToDecimal(accountSortino),
				DohWeek = string.IsNullOrEmpty(accountAvgDohWeek) ? null : (decimal?)Convert.ToDecimal(accountAvgDohWeek),
				DohDay = string.IsNullOrEmpty(accountAvgDohDay) ? null : (decimal?)Convert.ToDecimal(accountAvgDohDay),
			};

			return account;
		}

		public static ProductInfo FillProdactFromDB(SqlDataReader reader)
        {
            decimal? price = null;
            if (!string.IsNullOrEmpty(reader.GetValue(2).ToString()))
            {
                price = Convert.ToDecimal(reader.GetValue(2).ToString());
            }
            
            ProductInfo product = new ProductInfo()
            {
                Id = (int)reader.GetValue(0),
                Url = reader.GetValue(1).ToString(),
                Price = price,
                Category = reader.GetValue(3).ToString(),
                UrlPicture = reader.GetValue(4).ToString(),
                Vendor = reader.GetValue(5).ToString(),
                Model = reader.GetValue(6).ToString(),
                Description = reader.GetValue(7).ToString(),
                Name = reader.GetValue(8).ToString(),
                ProductCode = reader.GetValue(9).ToString(),
                isPrice = reader.GetValue(10).ToString() == "1",
                DealKeywords = reader.GetValue(11).ToString()
            };

            return product;
        }

        public static ProductFolder FillProdactFoldersFromDB(SqlDataReader reader)
        {
            int? parentId = null;
            if (!string.IsNullOrEmpty(reader.GetValue(2).ToString()))
                parentId = (int)reader.GetValue(2);

            ProductFolder productFolder = new ProductFolder()
            {
                Id = (int)reader.GetValue(0),
                FolderName = reader.GetValue(1).ToString(),
                ParentId = parentId
            };

            return productFolder;
        }

    }

    public class ExcelReportSheet
    {
        public string SheetName;
        public List<string> HeaderNames;
        public List<List<string>> ReportData;
    }
}
