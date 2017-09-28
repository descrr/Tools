using System;

namespace PriceGenerator
{
    public class ProductInfo
    {
        public string Name;
        public int Id;
        public string Url;
        public string Model;
        public decimal? Price;
        public string Description;
        public string Category;
        public string UrlPicture;
        public string ProductCode;
        public bool isPrice;

        public string Vendor;

        public string DealKeywords;

        public override string ToString()
        {
            return string.Format("{0}Name={1}, Id={2}, Url={3}, Price={4}", Environment.NewLine, Name, Id, Url, Price);
        }
    }

    public class WebPageInfo
    {
        public string Title;
        public string Price;
        public string Description;
        public string ParentTitle;
        public string ImageBox;
        public string ParamsTableVendor;
        public string ParamsProductCode;
    }

    public class Price
    {
        public string ProductCode;
        public decimal ProducrPrice;

        public Price(string productCode, decimal producrPrice)
        {
            ProductCode = productCode;
            ProducrPrice = producrPrice;
        }
    }

    public class ProductFolder
    {
        public int Id;
        public string FolderName;
        public int? ParentId;

    }
}
