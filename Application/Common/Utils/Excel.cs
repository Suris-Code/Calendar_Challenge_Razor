using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.DataValidation;
using Application.Common.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Common.Utils
{
    public class Excel
    {
        public static readonly Color HeaderBackgroundColor = Color.FromArgb(71, 71, 141);
        public static readonly Color HeaderFontColor = Color.FromArgb(255, 255, 255);

        public static void SetHyperlinkCell(ref ExcelWorksheet sheet, int row, int col, string? text, string sheetLink, string cellLink, bool bold)
        {
            SetHyperlinkCell(ref sheet, row, col, text, sheetLink, cellLink, bold, ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top);
        }

        public static void SetHyperlinkCell(ref ExcelWorksheet sheet, int row, int col, string? text, string sheetLink, string cellLink, bool bold,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {
            Color colorFont = Color.FromArgb(0, 0, 0);
            SetHyperlinkCell(ref sheet, row, col, text, sheetLink, cellLink, bold, colorFont, horizontalAlignment, verticalAlignment);
        }

        public static void SetHyperlinkCell(ref ExcelWorksheet sheet, int row, int col, string? text, string sheetLink, string cellLink, bool bold,
            Color colorFont,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {
            Color backgroundColor = Color.FromArgb(255, 255, 255);

            SetHyperlinkCell(ref sheet, row, col, text, sheetLink, cellLink, bold, colorFont, backgroundColor, horizontalAlignment, verticalAlignment);
        }

        public static void SetHyperlinkCell(ref ExcelWorksheet sheet, int row, int col, string? text, string sheetLink, string cellLink, bool bold,
            Color colorFont, Color backgroundColor)
        {
            SetHyperlinkCell(
                ref sheet, row, col, text, sheetLink, cellLink, bold, colorFont, backgroundColor,
                ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top);
        }

        public static void SetHyperlinkCell(ref ExcelWorksheet sheet, int row, int col, string? text, string sheetLink, string cellLink, bool bold,
            Color? colorFont, Color? backgroundColor,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {

            if(colorFont != null) sheet.Cells[row, col].Style.Font.Color.SetColor((Color)colorFont);
            sheet.Cells[row, col].Style.Font.Bold = bold;
            if(backgroundColor != null) sheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            if(backgroundColor != null) sheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor((Color)backgroundColor);
            sheet.Cells[row, col].Style.HorizontalAlignment = horizontalAlignment;
            sheet.Cells[row, col].Style.VerticalAlignment = verticalAlignment;

            sheet.Cells[row, col].Hyperlink = new ExcelHyperLink($"{(char)39}{sheetLink}{(char)39}!{cellLink}", text);
        }

        public static void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, object? value, bool bold)
        {
            SetValueCell(ref sheet, row, col, value, bold, ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top);
        }

        public static void SetValueCell(ref ExcelWorksheet sheet, int row, int col, object? value, bool bold,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {
            Color colorFont = Color.FromArgb(0, 0, 0);
            SetValueCell(ref sheet, row, col, value, bold, colorFont, horizontalAlignment, verticalAlignment);
        }

        public static void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, object? value, bool bold,
            Color colorFont,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {
            Color backgroundColor = Color.FromArgb(255, 255, 255);

            SetValueCell(ref sheet, row, col, value, bold, colorFont, backgroundColor, horizontalAlignment, verticalAlignment);
        }

        public static void SetValueCellBgd(
            ref ExcelWorksheet sheet, int row, int col, object? value, bool bold,
            Color backgroundColor,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {
            Color colorFont = Color.FromArgb(0, 0, 0);

            SetValueCell(ref sheet, row, col, value, bold, colorFont, backgroundColor, horizontalAlignment, verticalAlignment);
        }

        public static void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, object? value, bool bold,
            Color colorFont, Color backgroundColor)
        {
            SetValueCell(
                ref sheet, row, col, value, bold, colorFont, backgroundColor,
                ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top);
        }

        public static void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, object? value, bool bold,
            ExcelBorderStyle borderBottom)
        {
            SetValueCell(
                ref sheet, row, col, value, bold,
                Color.FromArgb(0, 0, 0), Color.FromArgb(255, 255, 255),
                ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top,
                null, borderBottom);
        }

        private void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, string? value, bool bold,
            Color colorFont,
            ExcelBorderStyle borderBottom)
        {
            SetValueCell(
                ref sheet, row, col, value, bold,
                colorFont, Color.FromArgb(255, 255, 255),
                ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top,
                null, borderBottom);
        }

        private void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, string? value, bool bold,
            Color colorFont, 
            ExcelBorderStyle? borderTop = null, ExcelBorderStyle? borderBottom = null)
        {
            Color backgroundColor = Color.FromArgb(255, 255, 255);

            SetValueCell(
                ref sheet, row, col, value, bold,
                colorFont, backgroundColor,
                ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top,
                borderTop, borderBottom);
        }

        private void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, string? value, bool bold,
            Color colorFont, Color backgroundColor,
            ExcelBorderStyle? borderTop = null, ExcelBorderStyle? borderBottom = null)
        {
            SetValueCell(
                ref sheet, row, col, value, bold,
                colorFont, backgroundColor,
                ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top,
                borderTop, borderBottom);
        }

        public static void SetValueCell(
            ref ExcelWorksheet sheet, int row, int col, object? value, bool bold,
            Color? colorFont, Color backgroundColor,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top,
            ExcelBorderStyle? borderTop = null, ExcelBorderStyle? borderBottom = null)
        {

            sheet.Cells[row, col].Value = value;
            if(colorFont != null) sheet.Cells[row, col].Style.Font.Color.SetColor((Color)colorFont);
            sheet.Cells[row, col].Style.Font.Bold = bold;
            if (backgroundColor != Color.White)
            {
                sheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor((Color)backgroundColor);
            }
            sheet.Cells[row, col].Style.HorizontalAlignment = horizontalAlignment;
            sheet.Cells[row, col].Style.VerticalAlignment = verticalAlignment;
            if(borderTop != null)
                sheet.Cells[row, col].Style.Border.Top.Style = borderTop ?? ExcelBorderStyle.None;
            if(borderBottom != null)
                sheet.Cells[row, col].Style.Border.Bottom.Style = borderBottom ?? ExcelBorderStyle.None;
            sheet.Cells[row, col].Style.HorizontalAlignment = horizontalAlignment;

            var valueType = value?.GetType();
            if(valueType == typeof(DateTime))
                sheet.Cells[row, col].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
            else if(valueType == typeof(decimal))
                sheet.Cells[row, col].Style.Numberformat.Format = "0.00";
            else if(valueType == typeof(int))
                sheet.Cells[row, col].Style.Numberformat.Format = "0";
            else if(valueType == typeof(bool))
                sheet.Cells[row, col].Style.Numberformat.Format = "0";
            else
                sheet.Cells[row, col].Style.Numberformat.Format = "@";
        }

        public static string IntegerValidation(string xCellAddress)
        {
            return String.Format("=AND(ISNUMBER({0}),({0}-INT({0})=0))", xCellAddress);
        }

        public static string DecimalValidation(string xCellAddress)
        {
            return String.Format("=ISNUMBER({0})", xCellAddress);
        }

        public static void SetFormatText(ref ExcelWorksheet sheet, int rowFrom, int rowTo, int colFrom, int colTo)
        {
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Numberformat.Format = "@";
        }

        public static void SetDecimalValidation(ref ExcelWorksheet sheet, int rowFrom, int rowTo, int colFrom, int colTo)
        {
            var validationVolume = sheet.DataValidations.AddCustomValidation($"{sheet.Cells[rowFrom, colFrom].Address}:{sheet.Cells[rowTo, colTo].Address}");
            validationVolume.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            validationVolume.Operator = ExcelDataValidationOperator.equal;
            validationVolume.ShowErrorMessage = true;
            validationVolume.ErrorTitle = "Decimal number";
            validationVolume.Error = "You must input an decimal value.";
            validationVolume.Formula.ExcelFormula = DecimalValidation($"{sheet.Cells[rowFrom, colFrom].Address}:{sheet.Cells[rowTo, colTo].Address}");
        }

        public static void SetIntegerValidation(ref ExcelWorksheet sheet, int rowFrom, int rowTo, int colFrom, int colTo)
        {
            var validationVolume = sheet.DataValidations.AddCustomValidation($"{sheet.Cells[rowFrom, colFrom].Address}:{sheet.Cells[rowTo, colTo].Address}");
            validationVolume.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            validationVolume.Operator = ExcelDataValidationOperator.equal;
            validationVolume.ShowErrorMessage = true;
            validationVolume.ErrorTitle = "Integer number";
            validationVolume.Error = "You must input an integer value.";
            validationVolume.Formula.ExcelFormula = IntegerValidation($"{sheet.Cells[rowFrom, colFrom].Address}:{sheet.Cells[rowTo, colTo].Address}");
        }

        public static void AddFormulaSum(ref ExcelWorksheet sheet, int row, int col, params int[] colSum)
        {
            sheet.Cells[$"{sheet.Cells[row, col].Address}"].Formula = "=";
            foreach (int colPos in colSum)
            {
                sheet.Cells[$"{sheet.Cells[row, col].Address}"].Formula += $"+{sheet.Cells[row, colPos].Address}";
            }
        }

        public static void Merged(ref ExcelWorksheet sheet, int rowFrom, int rowTo, int colFrom, int colTo,
            object? value, bool bold,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {
            Color fontColor = Color.FromArgb(0, 0, 0);

            Merged(ref sheet,
                rowFrom, rowTo, colFrom, colTo,
                value, bold, Color.FromArgb(0, 0, 0),
                horizontalAlignment, verticalAlignment);
        }

        public static void Merged(ref ExcelWorksheet sheet, int rowFrom, int rowTo, int colFrom, int colTo,
            object? value, bool bold, Color fontColor,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top)
        {
            Merged(ref sheet,
                rowFrom, rowTo, colFrom, colTo,
                value, bold, fontColor, Color.FromArgb(255, 255, 255),
                horizontalAlignment, verticalAlignment);
        }

        public static void Merged(ref ExcelWorksheet sheet, int rowFrom, int rowTo, int colFrom, int colTo,
            object? value, bool bold, Color fontColor, Color? backgroundColor,
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left,
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Top, 
            ExcelBorderStyle border = ExcelBorderStyle.None)
        {

            backgroundColor = backgroundColor == null ? Color.FromArgb(255, 255, 255) : backgroundColor;

            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Merge = true;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Value = value;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Font.Color.SetColor((Color)fontColor);
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Font.Bold = bold;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Fill.BackgroundColor.SetColor((Color)backgroundColor);
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.HorizontalAlignment = horizontalAlignment;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.VerticalAlignment = verticalAlignment;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Border.Bottom.Style = border;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Border.Top.Style = border;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Border.Left.Style = border;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.Border.Right.Style = border;
            sheet.Cells[rowFrom, colFrom, rowTo, colTo].Style.WrapText = true;
        }

        public static Tuple<bool, List<string>> ValidationHeader(ref ExcelWorksheet sheet, int row, int col, string headerText, Tuple<bool, List<string>> validation)
        {
            bool validationOk = validation.Item1;
            List<string> errors = validation.Item2;

            if (sheet.Cells[row, col] == null ||
                sheet.Cells[row, col].Value == null ||
                sheet.Cells[row, col].Value.ToString() != headerText)
            {
                errors.Add($"Expected header {headerText} in cell {sheet.Cells[row, col].Address} in sheet.");
                validationOk = false;
            }

            return new Tuple<bool, List<string>>(validationOk, errors);
        }


        public static Tuple<bool, List<string>> ValidationCellValue(ref ExcelWorksheet sheet, int row, int col, Tuple<bool, List<string>> validation)
        {
            bool validationOk = validation.Item1;
            List<string> errors = validation.Item2;

            if ((sheet.Cells[row, col] == null ||
                sheet.Cells[row, col].Value == null))
            {
                errors.Add($"Expected some value in cell {sheet.Cells[row, col].Address} in sheet.");
                validationOk = false;
            }

            return new Tuple<bool, List<string>>(validationOk, errors);
        }

        public static Tuple<bool, List<string>> ValidationCellNotNumeric(ref ExcelWorksheet sheet, int row, int col, Tuple<bool, List<string>> validation)
        {
            bool validationOk = validation.Item1;
            List<string> errors = validation.Item2;

            if (sheet.Cells[row, col] != null &&
                sheet.Cells[row, col].Value != null)
            {
                string? value = sheet.Cells[row, col].Value?.ToString();
                if (!double.TryParse(value, out _))
                {
                    errors.Add($"Value is text in cell {sheet.Cells[row, col].Address} in sheet.");
                    validationOk = false;
                }
            }

            return new Tuple<bool, List<string>>(validationOk, errors);
        }

        public static Result RecordFingerprint(ExcelWorksheet xExcelWorksheet, int xRow, int xColumn, string userFullName, string xFingerprint)
        {
            try
            {

                using (ExcelRange range = xExcelWorksheet.Cells[xRow, xColumn])
                {
                    range.Style.Font.Size = 8;
                    ExcelRichTextCollection RichTxtCollection = range.RichText;
                    ExcelRichText RichText = RichTxtCollection.Add($"Date : {DateTime.Now} - User: {userFullName} - Fingerprint: {xFingerprint} ");
                    RichText.Color = Color.Black;
                    RichText.Bold = false;

                    #region Comment          

                    ExcelComment comment = range.AddComment("This fingerprint is an unique hash code to indentificate this report file. The removal of this fingerprint will invalidate the report content.", "IFPI MAP");
                    ExcelRichTextCollection RichTxtCollectionComment = comment.RichText;
                    comment.RichText[0].PreserveSpace = false;
                    comment.Font.Bold = false;
                    comment.Font.Color = Color.Black;
                    #endregion
                }

                xExcelWorksheet.Cells[xRow, xColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                return Result.Success("");
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public static void SetCellBorderStyle(ExcelWorksheet sheet, int row, int col, ExcelBorderStyle borderStyle) 
        {
            sheet.Cells[row, col].Style.Border.Bottom.Style = borderStyle;
            sheet.Cells[row, col].Style.Border.Top.Style = borderStyle;
            sheet.Cells[row, col].Style.Border.Left.Style = borderStyle;
            sheet.Cells[row, col].Style.Border.Right.Style = borderStyle;
        }

        public static Color ParseColor(string colorString)
        {
            System.Text.RegularExpressions.Match rgbMatch = Regex.Match(colorString, @"^rgb\s*$\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*$$");
            if (rgbMatch.Success)
            {
                int red = int.Parse(rgbMatch.Groups[1].Value);
                int green = int.Parse(rgbMatch.Groups[2].Value);
                int blue = int.Parse(rgbMatch.Groups[3].Value);
                return Color.FromArgb(red, green, blue);
            }

            System.Text.RegularExpressions.Match hexMatch = Regex.Match(colorString, @"^#?([0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})$");
            if (hexMatch.Success)
            {
                string hex = hexMatch.Groups[1].Value;
                if (hex.Length == 3)
                {
                    hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
                }
                int rgb = int.Parse(hex, NumberStyles.HexNumber);
                return Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
            }

            return Color.White;
        }
    }
}
