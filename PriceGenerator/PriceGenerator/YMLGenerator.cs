using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Packaging;

namespace PriceGenerator
{
    public class YMLGenerator
    {
        public void Generate(List<List<string>> products, List<List<string>> folders, string filename)
        {
            string content = GetHeader();
            content += GetFolders(folders);
            content += GetProducts(products);
            content += GetTail();


            File.WriteAllText(filename, content);
        }

        private string GetHeader()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.AppendLine("<!DOCTYPE yml_catalog SYSTEM \"shops.dtd\">");
            builder.AppendLine(string.Format("<yml_catalog date=\"{0}\">", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            builder.AppendLine("    <shop>");
            builder.AppendLine("        <name>ООО Гипер</name>");
            builder.AppendLine("        <company>ООО Гипер</company>");
            builder.AppendLine("        <url>http://vofis.deal.by/</url>");
            builder.AppendLine("        <currencies>");
            builder.AppendLine("            <currency id=\"BYN\" rate=\"1\"/>");
            builder.AppendLine("        </currencies>");
            return builder.ToString();
        }

        private string GetFolders(List<List<string>> folders)
        {
            var builder = new StringBuilder();
            builder.AppendLine("        <categories>");

            foreach (var row in folders)
            {
                //hasn't parent?
                if(string.IsNullOrEmpty(row[4]))
                    builder.AppendLine(String.Format("            <category id=\"{0}\">{1}</category>", row[2], row[1]));
                else
                    builder.AppendLine(String.Format("            <category id=\"{0}\" parentId=\"{1}\">{2}</category>", row[2], row[4], row[1]));
            }

            builder.AppendLine("        </categories>");

            return builder.ToString();
        }

        private string GetProducts(List<List<string>> products)
        {
            var builder = new StringBuilder();
            builder.AppendLine("        <offers>");

            foreach (var row in products)
            {
                string dealKeywords = row[0].Replace(",", "").Replace("&", " ").Replace("«", "").Replace("»", "");
                if (!string.IsNullOrEmpty(row[15]))
                    dealKeywords = string.Format("{0},{1}", dealKeywords, row[15]);
                
                builder.AppendLine(String.Format("            <offer available=\"true\" selling_type=\"u\" id=\"{0}\">", row[10]));
                //builder.AppendLine(String.Format("            <url>{0}</url>", row[10]));
                builder.AppendLine(String.Format("           <price>{0}</price>", row[4]));
                builder.AppendLine(String.Format("           <prices>   <price>  <value>{0}</value>   <quantity>2</quantity>  </price> </prices>", row[4]));
                builder.AppendLine("           <currencyId>BYN</currencyId>");
                builder.AppendLine(String.Format("           <categoryId>{0}</categoryId>", row[11]));
                builder.AppendLine(String.Format("           <picture>{0}</picture>", row[8]));
                builder.AppendLine("           <pickup>true</pickup>");
                builder.AppendLine(String.Format("           <keywords>{0}</keywords>", dealKeywords));
                builder.AppendLine("           <delivery>true</delivery>");
                builder.AppendLine(String.Format("           <name>{0}</name>", GetName(row[0])));
                builder.AppendLine(String.Format("           <description><![CDATA[ {0} ]]></description>", row[2]));
                builder.AppendLine("            </offer>");
            }

            builder.AppendLine("        </offers>");

            return builder.ToString();
        }

        private string GetName(string name)
        {
            name = name.Replace("&", " ");
            //name = name.Replace("<", "&lt");
            //name = name.Replace(">", "&gt;");

            return name;
        }

        private string GetTail()
        {
            var builder = new StringBuilder();
            builder.AppendLine("    </shop>");
            builder.AppendLine("</yml_catalog>");
            return builder.ToString();
        }
    }
}
