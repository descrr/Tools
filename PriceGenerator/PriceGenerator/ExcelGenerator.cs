using System.Collections.Generic;
using System.IO;
using Luxottica.AdamExtensions.Maintenance.Jobs.DownloadViaFileConfiguration.Excel;
using Luxottica.AdamExtensions.Maintenance.Jobs.DownloadViaFileConfiguration.Helpers;

namespace PriceGenerator
{
    public class ExcelGenerator
    {
        public void GenerateExcel(List<ExcelData> sheets, string fileName)
        {
            var helper = new ExcelHelper();
            if (sheets.Count == 1)
            {
                ExcelData sheet = sheets[0];
                using (var stream = helper.CreateExcel(sheet.DataRows, sheet.SheetName, sheet.Headers))
                {
                    using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        stream.WriteTo(file);
                        file.Close();
                        stream.Close();
                    }
                }
            }
            else
            {
                using (var stream = helper.CreateExcelWithManySheets(sheets))
                {
                    using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        stream.WriteTo(file);
                        file.Close();
                        stream.Close();
                    }
                }
            }
        }
    }
}
