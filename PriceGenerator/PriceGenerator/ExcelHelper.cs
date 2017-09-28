using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using System.IO;

namespace PriceGenerator
{
    //public class ExcelHelper
    //{
    //    private string GetColumnName(string cellReference)
    //    {
    //        var regex = new Regex("[A-Za-z]+");
    //        var match = regex.Match(cellReference);

    //        return match.Value;
    //    }

    //    private int ConvertColumnNameToNumber(string columnName)
    //    {
    //        var alpha = new Regex("^[A-Z]+$");
    //        if (!alpha.IsMatch(columnName)) throw new ArgumentException();

    //        char[] colLetters = columnName.ToCharArray();
    //        Array.Reverse(colLetters);

    //        var convertedValue = 0;
    //        for (int i = 0; i < colLetters.Length; i++)
    //        {
    //            char letter = colLetters[i];
    //            // ASCII 'A' = 65
    //            int current = i == 0 ? letter - 65 : letter - 64;
    //            convertedValue += current * (int)Math.Pow(26, i);
    //        }

    //        return convertedValue;
    //    }

    //    private IEnumerator<Cell> GetExcelCellEnumerator(Row row)
    //    {
    //        int currentCount = 0;
    //        foreach (Cell cell in row.Descendants<Cell>())
    //        {
    //            string columnName = this.GetColumnName(cell.CellReference);

    //            int currentColumnIndex = this.ConvertColumnNameToNumber(columnName);

    //            for (; currentCount < currentColumnIndex; currentCount++)
    //            {
    //                var emptycell = new Cell()
    //                {
    //                    DataType = null,
    //                    CellValue = new CellValue(string.Empty)
    //                };
    //                yield return emptycell;
    //            }

    //            yield return cell;
    //            currentCount++;
    //        }
    //    }

    //    private string ReadExcelCell(Cell cell, WorkbookPart workbookPart)
    //    {
    //        var cellValue = cell.CellValue;
    //        var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
    //        if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
    //        {
    //            text = workbookPart.SharedStringTablePart.SharedStringTable
    //                .Elements<SharedStringItem>().ElementAt(
    //                    Convert.ToInt32(cell.CellValue.Text)).InnerText;
    //        }
    //        else if (cell.StyleIndex != null && cell.StyleIndex.HasValue)
    //        {
    //            try
    //            {
    //                var styleIndex = (int)cell.StyleIndex.Value;
    //                var stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;
    //                var cellFormat = stylesheet.CellFormats.Elements<CellFormat>().ElementAt(styleIndex);
    //                var numberingFormat = stylesheet.NumberingFormats.Elements<NumberingFormat>().Where(x => x.NumberFormatId.Value == cellFormat.NumberFormatId).FirstOrDefault();
    //                if (numberingFormat != null)
    //                {
    //                    int value;
    //                    if (Int32.TryParse(text, out value))
    //                    {
    //                        text = value.ToString(numberingFormat.FormatCode);
    //                    }
    //                }
    //            }
    //            catch (Exception)
    //            {
    //            }
    //        }

    //        return (text ?? string.Empty).Trim();
    //    }


    //    public void CreateExcel(
    //        string fileName,
    //        List<ExcelReportSheet> ReportSheets
    //        //List<List<string>> data,
    //        //string sheetName,
    //        //List<string> headerNames
    //            )
    //    {
    //        //Open the copied template workbook. 
    //        using (SpreadsheetDocument myWorkbook =
    //               SpreadsheetDocument.Create(fileName,
    //               SpreadsheetDocumentType.Workbook))
    //        {

    //            WorkbookPart workbookPart = myWorkbook.AddWorkbookPart();
    //            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
    //            SharedStringTablePart shareStringPart = this.CreateSharedStringTablePart(workbookPart);

    //            // Create Styles and Insert into Workbook
    //            var stylesPart =
    //                myWorkbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
    //            Stylesheet styles = new ExcelStylesheet();
    //            styles.Save(stylesPart);
    //            string relId = workbookPart.GetIdOfPart(worksheetPart);
    //            var workbook = new Workbook();
    //            var fileVersion =
    //                new FileVersion
    //                {
    //                    ApplicationName =
    //                        "Microsoft Office Excel"
    //                };

    //            foreach (var reportSheet in ReportSheets)
    //            {
    //                var worksheet = new Worksheet();
    //                int numCols = reportSheet.HeaderNames.Count;
    //                var columns = new Columns();
    //                var maxLength = reportSheet.HeaderNames.Max(x => x.Length);
    //                for (int col = 0; col < numCols; col++)
    //                {
    //                    int width = maxLength + 5; //headerNames[col].Length + 5;
    //                    Column c = new ExcelColumn((UInt32)col + 1,
    //                        (UInt32)numCols + 1, width);
    //                    columns.Append(c);
    //                }
    //                worksheet.Append(columns);
    //                var sheets = new Sheets();
    //                var sheet = new Sheet { Name = reportSheet.SheetName, SheetId = 1, Id = relId };
    //                sheets.Append(sheet);
    //                workbook.Append(fileVersion);
    //                workbook.Append(sheets);

    //                SheetData sheetData = WriteSheetData(reportSheet.ReportData, reportSheet.HeaderNames, shareStringPart);
    //                worksheet.Append(sheetData);
    //                worksheetPart.Worksheet = worksheet;
    //                worksheetPart.Worksheet.Save();
    //            }

    //            myWorkbook.WorkbookPart.Workbook = workbook;
    //            myWorkbook.WorkbookPart.Workbook.Save();
    //            myWorkbook.Close();
    //        }
    //    }

    //    private SharedStringTablePart CreateSharedStringTablePart(WorkbookPart workbookPart)
    //    {
    //        SharedStringTablePart shareStringPart;
    //        if (workbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
    //        {
    //            shareStringPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
    //        }
    //        else
    //        {
    //            shareStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
    //        }
    //        if (shareStringPart.SharedStringTable == null)
    //        {
    //            shareStringPart.SharedStringTable = new SharedStringTable();
    //        }
    //        return shareStringPart;
    //    }

    //    private static SheetData WriteSheetData(List<List<string>> data, List<string> headerNames, SharedStringTablePart shareStringPart)
    //    {
    //        var sheetData = new SheetData();
    //        //Get a list of A to Z
    //        var az = new List<Char>(Enumerable.Range('A', 'Z' -
    //                              'A' + 1).Select(i => (Char)i).ToArray());
    //        //A to E number of columns 
    //        List<Char> headers = az.GetRange(0, headerNames.Count);
    //        int numRows = data.Count;
    //        int numCols = headerNames.Count;
    //        var header = new Row();
    //        int index = 1;
    //        header.RowIndex = (uint)index;
    //        for (int col = 0; col < numCols; col++)
    //        {
    //            var c = new SharedStringTextCell(headers[col].ToString(),
    //                                   headerNames[col], index, shareStringPart);
    //            c.StyleIndex = 11;
    //            header.Append(c);
    //        }
    //        sheetData.Append(header);

    //        for (int i = 0; i < numRows; i++)
    //        {
    //            index++;
    //            var dataItem = data[i];
    //            var r = new Row { RowIndex = (uint)index };
    //            for (int col = 0; col < numCols; col++)
    //            {
    //                //var c = new TextCell(headers[col].ToString(),
    //                //                            dataItem[col], index);
    //                var c = new SharedStringTextCell(headers[col].ToString(), dataItem[col], index, shareStringPart);
    //                r.Append(c);
    //            }
    //            sheetData.Append(r);
    //        }
    //        return sheetData;
    //    }

    //    public MemoryStream CreateExcel(List<List<string>> data, string sheetName, List<string> headerNames)
    //    {
    //        var stream = new MemoryStream();
    //        //Open the copied template workbook. 
    //        using (SpreadsheetDocument myWorkbook =
    //               SpreadsheetDocument.Create(stream,
    //               SpreadsheetDocumentType.Workbook))
    //        {

    //            WorkbookPart workbookPart = myWorkbook.AddWorkbookPart();
    //            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
    //            SharedStringTablePart shareStringPart = this.CreateSharedStringTablePart(workbookPart);

    //            // Create Styles and Insert into Workbook
    //            var stylesPart =
    //                myWorkbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
    //            Stylesheet styles = new ExcelStylesheet();
    //            styles.Save(stylesPart);
    //            string relId = workbookPart.GetIdOfPart(worksheetPart);
    //            var workbook = new Workbook();
    //            var fileVersion =
    //                new FileVersion
    //                {
    //                    ApplicationName =
    //                        "Microsoft Office Excel"
    //                };
    //            var worksheet = new Worksheet();
    //            int numCols = headerNames.Count;
    //            var columns = new Columns();
    //            var maxLength = headerNames.Max(x => x.Length);
    //            for (int col = 0; col < numCols; col++)
    //            {
    //                int width = maxLength + 5;//headerNames[col].Length + 5;
    //                Column c = new ExcelColumn((UInt32)col + 1,
    //                              (UInt32)numCols + 1, width);
    //                columns.Append(c);
    //            }
    //            worksheet.Append(columns);

    //            var sheets = new Sheets();
    //            var sheet = new Sheet { Name = sheetName, SheetId = 1, Id = relId };
    //            sheets.Append(sheet);
    //            workbook.Append(fileVersion);
    //            workbook.Append(sheets);

    //            SheetData sheetData = WriteSheetData(data, headerNames, shareStringPart);
    //            worksheet.Append(sheetData);

    //            worksheetPart.Worksheet = worksheet;
    //            worksheetPart.Worksheet.Save();
    //            myWorkbook.WorkbookPart.Workbook = workbook;
    //            myWorkbook.WorkbookPart.Workbook.Save();
    //            myWorkbook.Close();
    //        }

    //        return stream;
    //    }

    //    public MemoryStream CreateExcelWithManySheets(IEnumerable<ExcelData> data)
    //    {
    //        var stream = new MemoryStream();
    //        using (var spreadSheetDoc = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
    //        {
    //            var workbookPart = spreadSheetDoc.AddWorkbookPart();
    //            workbookPart.Workbook = new Workbook { Sheets = new Sheets() };

    //            var stylesPart = spreadSheetDoc.WorkbookPart.AddNewPart<WorkbookStylesPart>();
    //            stylesPart.Stylesheet = CreateStylesheet(12, 10);
    //            //styles.Save(stylesPart);

    //            foreach (var excelData in data)
    //            {
    //                CreateReportSheet(workbookPart, excelData.SheetName, excelData.Headers, excelData.DataRows);
    //            }

    //            workbookPart.Workbook.Save();
    //        }
    //        return stream;
    //    }

    //    private void CreateReportSheet(WorkbookPart workbookpart, string sheetName, IReadOnlyCollection<string> headers, IEnumerable<IEnumerable<string>> data)
    //    {
    //        var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
    //        worksheetPart.Worksheet = new Worksheet();
    //        var sheetData = new SheetData();

    //        var sheets = workbookpart.Workbook.Sheets;
    //        var sheetId = Convert.ToUInt32(sheets.Count()) + 1;
    //        var sheet = new Sheet
    //        {
    //            Id = workbookpart.GetIdOfPart(worksheetPart),
    //            SheetId = sheetId,
    //            Name = sheetName
    //        };
    //        sheets.AppendChild(sheet);

    //        var columnWidth = headers.Max(x => x.Length);

    //        // Add header
    //        uint rowIndex = 0;
    //        var row = new Row { RowIndex = ++rowIndex };
    //        sheetData.AppendChild(row);
    //        var cellIndex = 0;
    //        Cell cell;

    //        foreach (var header in headers)
    //        {
    //            cell = CreateTextCell(ColumnLetter(cellIndex++), rowIndex, header);
    //            cell.StyleIndex = 1;
    //            row.AppendChild(cell);
    //        }

    //        // Add sheet data
    //        foreach (var item in data)
    //        {
    //            cellIndex = 0;
    //            row = new Row { RowIndex = ++rowIndex };
    //            sheetData.AppendChild(row);
    //            foreach (var str in item)
    //            {
    //                cell = CreateTextCell(ColumnLetter(cellIndex++), rowIndex, str);
    //                row.AppendChild(cell);
    //            }

    //            columnWidth = UpdateColumnWidth(columnWidth, item);
    //        }

    //        //Add columns
    //        var columns = GenerateColumns(columnWidth + 5, headers.Count);
    //        worksheetPart.Worksheet.Append(columns);

    //        worksheetPart.Worksheet.Append(sheetData);
    //    }

    //    private Cell CreateTextCell(string header, uint index, string text)
    //    {
    //        var cell = new Cell
    //        {
    //            DataType = CellValues.InlineString,
    //            CellReference = header + index
    //        };

    //        var istring = new InlineString();
    //        var t = new Text { Text = text };
    //        istring.AppendChild(t);
    //        cell.AppendChild(istring);
    //        return cell;
    //    }

    //    private string ColumnLetter(int intCol)
    //    {
    //        var intFirstLetter = ((intCol) / 676) + 64;
    //        var intSecondLetter = ((intCol % 676) / 26) + 64;
    //        var intThirdLetter = (intCol % 26) + 65;

    //        var firstLetter = (intFirstLetter > 64) ? (char)intFirstLetter : ' ';
    //        var secondLetter = (intSecondLetter > 64) ? (char)intSecondLetter : ' ';
    //        var thirdLetter = (char)intThirdLetter;

    //        return string.Concat(firstLetter, secondLetter, thirdLetter).Trim();
    //    }

    //    private Columns GenerateColumns(int width, int columnsCount)
    //    {
    //        var xlCols = new Columns();
    //        var xlCol = new Column
    //        {
    //            Min = 1U,
    //            Max = Convert.ToUInt32(columnsCount),
    //            Width = width,
    //            CustomWidth = true,
    //            BestFit = true
    //        };
    //        xlCols.Append(xlCol);
    //        return xlCols;
    //    }

    //    private int UpdateColumnWidth(int columnWidth, IEnumerable<string> item)
    //    {
    //        try
    //        {
    //            var maxLength = item.Max(x => x.Length);
    //            if (maxLength > columnWidth) columnWidth = maxLength;
    //        }
    //        catch (Exception)
    //        {
    //            // ignored
    //        }
    //        return columnWidth;
    //    }

    //    private Stylesheet CreateStylesheet(int headerFontSize, int regularFontSize)
    //    {
    //        var stylesheet = new Stylesheet { Fonts = new Fonts() };
    //        // blank font list
    //        stylesheet.Fonts.AppendChild(new Font { FontSize = new FontSize { Val = regularFontSize } });
    //        stylesheet.Fonts.AppendChild(new Font { Bold = new Bold(), FontSize = new FontSize { Val = headerFontSize } });
    //        stylesheet.Fonts.Count = 2;

    //        // create fills
    //        stylesheet.Fills = new Fills();

    //        // create a solid red fill
    //        var solidGray = new PatternFill
    //        {
    //            PatternType = PatternValues.Solid,
    //            ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("E7E6E6") },
    //            BackgroundColor = new BackgroundColor { Indexed = 64 }
    //        };
    //        // gray fill

    //        stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
    //        stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
    //        stylesheet.Fills.AppendChild(new Fill { PatternFill = solidGray });
    //        stylesheet.Fills.Count = 3;
    //        //stylesheet.Fills.Count = 2;

    //        // blank border list
    //        stylesheet.Borders = new Borders { Count = 1 };
    //        stylesheet.Borders.AppendChild(new Border());

    //        // blank cell format list
    //        stylesheet.CellStyleFormats = new CellStyleFormats { Count = 1 };
    //        stylesheet.CellStyleFormats.AppendChild(new CellFormat());

    //        // cell format list
    //        stylesheet.CellFormats = new CellFormats();
    //        // empty one for index 0, seems to be required
    //        stylesheet.CellFormats.AppendChild(new CellFormat());
    //        // cell format references style format 0, font 0, border 0, fill 2 and applies the fill
    //        stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 1, BorderId = 0, FillId = 2, ApplyFill = true });
    //        //stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 1, BorderId = 0, FillId = 2, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });
    //        stylesheet.CellFormats.Count = 2;

    //        return stylesheet;
    //    }
    //}

    //public class ExcelStylesheet : Stylesheet
    //{
    //    public ExcelStylesheet()
    //    {
    //        var fonts = new Fonts();
    //        var font = new DocumentFormat.OpenXml.Spreadsheet.Font();
    //        var fontName = new FontName { Val = StringValue.FromString("Arial") };
    //        var fontSize = new FontSize { Val = DoubleValue.FromDouble(11) };
    //        font.FontName = fontName;
    //        font.FontSize = fontSize;
    //        fonts.Append(font);
    //        //Font Index 1
    //        font = new DocumentFormat.OpenXml.Spreadsheet.Font();
    //        fontName = new FontName { Val = StringValue.FromString("Arial") };
    //        fontSize = new FontSize { Val = DoubleValue.FromDouble(12) };
    //        font.FontName = fontName;
    //        font.FontSize = fontSize;
    //        font.Bold = new Bold();
    //        fonts.Append(font);
    //        fonts.Count = UInt32Value.FromUInt32((uint)fonts.ChildElements.Count);
    //        var fills = new Fills();
    //        var fill = new Fill();
    //        var patternFill = new PatternFill { PatternType = PatternValues.None };
    //        fill.PatternFill = patternFill;
    //        fills.Append(fill);
    //        fill = new Fill();
    //        patternFill = new PatternFill { PatternType = PatternValues.Gray125 };
    //        fill.PatternFill = patternFill;
    //        fills.Append(fill);
    //        //Fill index  2
    //        fill = new Fill();
    //        patternFill = new PatternFill
    //        {
    //            PatternType = PatternValues.Solid,
    //            ForegroundColor = new ForegroundColor()
    //        };
    //        patternFill.ForegroundColor =
    //           TranslateForeground(System.Drawing.Color.LightBlue);
    //        patternFill.BackgroundColor =
    //            new BackgroundColor { Rgb = patternFill.ForegroundColor.Rgb };
    //        fill.PatternFill = patternFill;
    //        fills.Append(fill);
    //        //Fill index  3
    //        fill = new Fill();
    //        patternFill = new PatternFill
    //        {
    //            PatternType = PatternValues.Solid,
    //            ForegroundColor = new ForegroundColor()
    //        };
    //        patternFill.ForegroundColor =
    //           TranslateForeground(System.Drawing.Color.DodgerBlue);
    //        patternFill.BackgroundColor =
    //           new BackgroundColor { Rgb = patternFill.ForegroundColor.Rgb };
    //        fill.PatternFill = patternFill;
    //        fills.Append(fill);
    //        fills.Count = UInt32Value.FromUInt32((uint)fills.ChildElements.Count);
    //        var borders = new Borders();
    //        var border = new Border
    //        {
    //            LeftBorder = new LeftBorder(),
    //            RightBorder = new RightBorder(),
    //            TopBorder = new TopBorder(),
    //            BottomBorder = new BottomBorder(),
    //            DiagonalBorder = new DiagonalBorder()
    //        };
    //        borders.Append(border);
    //        //All Boarder Index 1
    //        border = new Border
    //        {
    //            LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin },
    //            RightBorder = new RightBorder { Style = BorderStyleValues.Thin },
    //            TopBorder = new TopBorder { Style = BorderStyleValues.Thin },
    //            BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin },
    //            DiagonalBorder = new DiagonalBorder()
    //        };
    //        borders.Append(border);
    //        //Top and Bottom Boarder Index 2
    //        border = new Border
    //        {
    //            LeftBorder = new LeftBorder(),
    //            RightBorder = new RightBorder(),
    //            TopBorder = new TopBorder { Style = BorderStyleValues.Thin },
    //            BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin },
    //            DiagonalBorder = new DiagonalBorder()
    //        };
    //        borders.Append(border);
    //        borders.Count = UInt32Value.FromUInt32((uint)borders.ChildElements.Count);
    //        var cellStyleFormats = new CellStyleFormats();
    //        var cellFormat = new CellFormat
    //        {
    //            NumberFormatId = 0,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0
    //        };
    //        cellStyleFormats.Append(cellFormat);
    //        cellStyleFormats.Count =
    //           UInt32Value.FromUInt32((uint)cellStyleFormats.ChildElements.Count);
    //        uint iExcelIndex = 164;
    //        var numberingFormats = new NumberingFormats();
    //        var cellFormats = new CellFormats();
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = 0,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0
    //        };
    //        cellFormats.Append(cellFormat);
    //        var nformatDateTime = new NumberingFormat
    //        {
    //            NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
    //            FormatCode = StringValue.FromString("dd/mm/yyyy hh:mm:ss")
    //        };
    //        numberingFormats.Append(nformatDateTime);
    //        var nformat4Decimal = new NumberingFormat
    //        {
    //            NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
    //            FormatCode = StringValue.FromString("#,##0.0000")
    //        };
    //        numberingFormats.Append(nformat4Decimal);
    //        var nformat2Decimal = new NumberingFormat
    //        {
    //            NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
    //            FormatCode = StringValue.FromString("#,##0.00")
    //        };
    //        numberingFormats.Append(nformat2Decimal);
    //        var nformatForcedText = new NumberingFormat
    //        {
    //            NumberFormatId = UInt32Value.FromUInt32(iExcelIndex),
    //            FormatCode = StringValue.FromString("@")
    //        };
    //        numberingFormats.Append(nformatForcedText);
    //        // index 1
    //        // Cell Standard Date format 
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = 14,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 2
    //        // Cell Standard Number format with 2 decimal placing
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = 4,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 3
    //        // Cell Date time custom format
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformatDateTime.NumberFormatId,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 4
    //        // Cell 4 decimal custom format
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformat4Decimal.NumberFormatId,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 5
    //        // Cell 2 decimal custom format
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformat2Decimal.NumberFormatId,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 6
    //        // Cell forced number text custom format
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformatForcedText.NumberFormatId,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 7
    //        // Cell text with font 12 
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformatForcedText.NumberFormatId,
    //            FontId = 1,
    //            FillId = 0,
    //            BorderId = 0,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 8
    //        // Cell text
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformatForcedText.NumberFormatId,
    //            FontId = 0,
    //            FillId = 0,
    //            BorderId = 1,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 9
    //        // Coloured 2 decimal cell text
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformat2Decimal.NumberFormatId,
    //            FontId = 0,
    //            FillId = 2,
    //            BorderId = 2,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 10
    //        // Coloured cell text
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformatForcedText.NumberFormatId,
    //            FontId = 0,
    //            FillId = 2,
    //            BorderId = 2,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        // Index 11
    //        // Coloured cell text
    //        cellFormat = new CellFormat
    //        {
    //            NumberFormatId = nformatForcedText.NumberFormatId,
    //            FontId = 1,
    //            FillId = 3,
    //            BorderId = 2,
    //            FormatId = 0,
    //            ApplyNumberFormat = BooleanValue.FromBoolean(true)
    //        };
    //        cellFormats.Append(cellFormat);
    //        numberingFormats.Count =
    //          UInt32Value.FromUInt32((uint)numberingFormats.ChildElements.Count);
    //        cellFormats.Count = UInt32Value.FromUInt32((uint)cellFormats.ChildElements.Count);
    //        this.Append(numberingFormats);
    //        this.Append(fonts);
    //        this.Append(fills);
    //        this.Append(borders);
    //        this.Append(cellStyleFormats);
    //        this.Append(cellFormats);
    //        var css = new CellStyles();
    //        var cs = new CellStyle
    //        {
    //            Name = StringValue.FromString("Normal"),
    //            FormatId = 0,
    //            BuiltinId = 0
    //        };
    //        css.Append(cs);
    //        css.Count = UInt32Value.FromUInt32((uint)css.ChildElements.Count);
    //        this.Append(css);
    //        var dfs = new DifferentialFormats { Count = 0 };
    //        this.Append(dfs);
    //        var tss = new TableStyles
    //        {
    //            Count = 0,
    //            DefaultTableStyle = StringValue.FromString("TableStyleMedium9"),
    //            DefaultPivotStyle = StringValue.FromString("PivotStyleLight16")
    //        };
    //        this.Append(tss);
    //    }

    //    private static ForegroundColor TranslateForeground(System.Drawing.Color fillColor)
    //    {
    //        return new ForegroundColor()
    //        {
    //            Rgb = new HexBinaryValue()
    //            {
    //                Value =
    //                    System.Drawing.ColorTranslator.ToHtml(
    //                    System.Drawing.Color.FromArgb(
    //                        fillColor.A,
    //                        fillColor.R,
    //                        fillColor.G,
    //                        fillColor.B)).Replace("#", "")
    //            }
    //        };
    //    }
    //}

    //public class ExcelColumn : Column
    //{
    //    public ExcelColumn(UInt32 startColumnIndex,
    //           UInt32 endColumnIndex, double columnWidth)
    //    {
    //        this.Min = startColumnIndex;
    //        this.Max = endColumnIndex;
    //        this.Width = columnWidth;
    //        this.CustomWidth = true;
    //    }
    //}

    //public class SharedStringTextCell : Cell
    //{
    //    public SharedStringTextCell(string header, string text, int index, SharedStringTablePart shareStringPart)
    //    {
    //        int cellIndex = InsertSharedStringItem(text, shareStringPart);
    //        this.CellValue = new CellValue(cellIndex.ToString());
    //        this.DataType = CellValues.SharedString;
    //        this.CellReference = header + index;
    //    }

    //    private int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
    //    {
    //        int i = 0;

    //        foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
    //        {
    //            if (item.InnerText == text)
    //            {
    //                return i;
    //            }

    //            i++;
    //        }

    //        shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
    //        shareStringPart.SharedStringTable.Save();

    //        return i;
    //    }
    //}

    //public class ExcelData
    //{
    //    public ExcelStatus Status { get; set; }
    //    public Columns ColumnConfigurations { get; set; }
    //    public List<string> Headers { get; set; }
    //    public List<List<string>> DataRows { get; set; }
    //    public string SheetName { get; set; }

    //    public ExcelData()
    //    {
    //        Status = new ExcelStatus();
    //        Headers = new List<string>();
    //        DataRows = new List<List<string>>();
    //    }
    //}

    //public class ExcelStatus
    //{
    //    public string Message { get; set; }
    //    public bool Success
    //    {
    //        get { return string.IsNullOrWhiteSpace(Message); }
    //    }
    //}
}