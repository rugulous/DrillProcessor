using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;

namespace DrillProcessor
{
    internal static class Helpers
    {
        public static int Scale(decimal num, int scale)
        {
            return (int)(num * scale);
        }

        public static int Scale(double num, int scale)
        {
            return (int) (num * scale);
        }

        public static int Scale(int num, int scale)
        {
            return num * scale;
        }

        public static string ExtractText(string file, int? pageLimit = null)
        {
            PdfDocument pdf = new(new PdfReader(file));
            int totalPages = pdf.GetNumberOfPages();

            if(pageLimit == null || pageLimit > totalPages || pageLimit <= 0)
            {
                pageLimit = totalPages;
            }

            StringBuilder drillBuilder = new();
            for (int i = 1; i <= pageLimit; i++)
            {
                var page = pdf.GetPage(i);
                string text = PdfTextExtractor.GetTextFromPage(page);
                drillBuilder.AppendLine(text);
            }
            pdf.Close();

            return drillBuilder.ToString().Trim();
        }
    }
}
