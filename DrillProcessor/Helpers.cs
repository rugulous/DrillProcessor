using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Geom;

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
            return (int)(num * scale);
        }

        public static int Scale(int num, int scale)
        {
            return num * scale;
        }

        public static string ExtractText(string file, int? pageLimit = null, bool twoToAPage = false)
        {
            PdfDocument pdf = new(new PdfReader(file));
            int totalPages = pdf.GetNumberOfPages();

            if (pageLimit == null || pageLimit > totalPages || pageLimit <= 0)
            {
                pageLimit = totalPages;
            }

            StringBuilder drillBuilder = new();
            for (int i = 1; i <= pageLimit; i++)
            {
                var page = pdf.GetPage(i);

                if (twoToAPage)
                {
                    Rectangle fullPage = page.GetMediaBox();
                    float targetW = fullPage.GetWidth() / 2;
                    float targetH = fullPage.GetHeight() / 2;

                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            Rectangle target = new(x * targetW, y * targetH, targetW, targetH);
                            FilteredTextEventListener stratagem = new(new LocationTextExtractionStrategy(), new TextRegionEventFilter(target));
                            drillBuilder.AppendLine(PdfTextExtractor.GetTextFromPage(page, stratagem));
                        }
                    }
                }
                else
                {
                    drillBuilder.AppendLine(PdfTextExtractor.GetTextFromPage(page));
                }
            }
            pdf.Close();

            return drillBuilder.ToString().Trim();
        }
    }
}
