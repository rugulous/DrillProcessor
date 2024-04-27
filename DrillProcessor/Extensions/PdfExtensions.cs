using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;

namespace DrillProcessor.Extensions
{
    public static class PdfExtensions
    {
        public static PdfDocument SplitVertically(this PdfDocument pdf)
        {
            Stream dummy = new MemoryStream();
            PdfWriter writer = new PdfWriter(dummy);
            writer.SetCloseStream(false);
            PdfDocument copy = new(writer);

            for(int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                PdfPage currPage = pdf.GetPage(i);
                Rectangle pageSize = currPage.GetMediaBox();
                Rectangle newTarget = new Rectangle(pageSize.GetLeft(), pageSize.GetTop(), pageSize.GetWidth() / 2, pageSize.GetHeight() / 2);

                PdfPage left = currPage.CopyTo(copy);
                left.SetMediaBox(newTarget);

                PdfPage right = currPage.CopyTo(copy);
                right.SetMediaBox(newTarget);
            }

            copy.Close();
            
            PdfDocument readablePdf = new(new PdfReader(dummy));

            return readablePdf;
        }
    }
}
