using System;
using System.Collections.Generic;
using System.Data;
using ExcelLibrary.Office.Excel;

namespace PriceGenerator
{
    public class ExcelWriter
    {
        public void Generate(string filename, List<ExcelReportSheet> ReportSheets)
        {
            var workbook = new Workbook();
            foreach (var sheet in ReportSheets)
            {
                var worksheet = new Worksheet(sheet.SheetName);
                for (int i = 0; i < sheet.HeaderNames.Count; i++)
                    worksheet.Cells[0, i] = new Cell(sheet.HeaderNames[i]);

                int rowIndex = 1;
                foreach (var row in sheet.ReportData)
                {
                    int columnIndex = 0;
                    foreach (var column in row)
                    {
                        worksheet.Cells[rowIndex, columnIndex++] = new Cell(column);
                    }
                    ++rowIndex;
                }

                workbook.Worksheets.Add(worksheet);
            }

            workbook.Save(filename);
        }
    }
}
