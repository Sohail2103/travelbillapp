using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using TravelBillApp.Models;

namespace TravelBillApp.Services;

public static class WatermarkRemover
{
    /// <summary>
    /// Opens an existing PDF and draws a solid white rectangle over the bottom
    /// strip of every page, covering footer watermarks (e.g. "Edit with WPS
    /// Office") that are baked into the page content. This does not alter the
    /// original text/content above the strip.
    /// </summary>
    public static byte[] RemoveBottomWatermark(byte[] inputPdfBytes, WatermarkRemovalOptions options)
    {
        using var inputStream = new MemoryStream(inputPdfBytes);
        using var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        foreach (var page in document.Pages)
        {
            // XGraphicsPdfPageOptions.Append draws ON TOP of the existing page
            // content instead of replacing it — critical, otherwise the letter
            // text itself would be wiped out along with the watermark.
            using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

            double stripHeight = Math.Min(options.BottomStripHeight, page.Height.Point);
            double y = page.Height.Point - stripHeight;

            gfx.DrawRectangle(XBrushes.White, 0, y, page.Width.Point, stripHeight);
        }

        using var outputStream = new MemoryStream();
        document.Save(outputStream, false);
        return outputStream.ToArray();
    }
}
