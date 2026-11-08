using Expenses.Core;
using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Expenses.App;

internal static class PdfExport
{
    public static void Write(string path, string title, IReadOnlyList<Expense> expenses)
    {
        using var document = new PdfDocument();
        document.Info.Title = title;
        var font = new XFont("Arial", 10);
        var heading = new XFont("Arial", 14, XFontStyleEx.Bold);
        XGraphics? graphics = null;
        double y = 0;
        int pageNumber = 0;
        void NewPage()
        {
            graphics?.Dispose();
            var page = document.AddPage();
            graphics = XGraphics.FromPdfPage(page);
            graphics.DrawString(title, heading, XBrushes.Black, new XRect(35, 25, page.Width.Point - 70, 24), XStringFormats.TopLeft);
            graphics.DrawString($"Bookkeeping summary - USD - Page {++pageNumber}", font, XBrushes.Black, 35, 65);
            graphics.DrawString("Business portion is not a determination of tax deductibility.", font, XBrushes.Black, 35, 82);
            y = 105;
        }
        void Line(string text)
        {
            if (graphics is null || y > document.Pages[document.PageCount - 1].Height.Point - 45) NewPage();
            graphics!.DrawString(text, font, XBrushes.Black, new XRect(35, y, document.Pages[document.PageCount - 1].Width.Point - 70, 17), XStringFormats.TopLeft);
            y += 17;
        }
        void Wrapped(string text)
        {
            while (text.Length > 0)
            {
                if (graphics is null) NewPage();
                double width = document.Pages[document.PageCount - 1].Width.Point - 70;
                int count = text.Length;
                while (count > 1 && graphics!.MeasureString(text[..count], font).Width > width) count--;
                Line(text[..count]);
                text = text[count..];
            }
        }
        try
        {
            NewPage();
            foreach (var expense in expenses)
            {
                Line($"{expense.PaidDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}   Amount: {expense.Amount:N2}   Business: {expense.BusinessAmount:N2} ({expense.BusinessUsePercent}%)");
                Wrapped($"{expense.Payee} | {expense.Category} | {expense.Description}".Replace("\r", " ").Replace("\n", " "));
                if (expense.Tags.Length > 0) Wrapped("Tags: " + expense.Tags);
                if (expense.PaymentReference.Length > 0) Wrapped("Payment reference: " + expense.PaymentReference);
                if (expense.ReceiptPath.Length > 0) Wrapped("Receipt: " + expense.ReceiptPath);
                Line("");
            }
            Line($"Recorded total: {expenses.Sum(e => e.Amount):N2} USD");
            Line($"Business portion: {expenses.Sum(e => e.BusinessAmount):N2} USD");
            graphics!.Dispose();
            graphics = null;
            document.Save(path);
        }
        finally { graphics?.Dispose(); }
    }
}
