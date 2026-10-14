using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using TravelBillApp.Models;

namespace TravelBillApp.Services;

public static class PdfDocumentGenerator
{
    private const double MarginLeft = 60;
    private const double MarginRight = 60;
    private const double MarginTop = 60;
    private const double MarginBottom = 60;
    private const double LineSpacing = 1.4;

    public static byte[] Generate(PdfDocumentRequest request)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var boldFont = new XFont("Times New Roman", 12, XFontStyle.Bold);
        var regularFont = new XFont("Times New Roman", 12, XFontStyle.Regular);

        double pageHeight = page.Height;
        double usableWidth = page.Width - MarginLeft - MarginRight;
        double y = MarginTop;

        // --- Header: To / From ---
        (page, gfx, y) = WriteBlock(document, page, gfx, y, pageHeight, regularFont, "To:", request.ToName, request.ToLine2);
        y += regularFont.GetHeight() * 0.6;
        (page, gfx, y) = WriteBlock(document, page, gfx, y, pageHeight, regularFont, "From:", request.FromName, request.FromLine2);
        y += regularFont.GetHeight() * 0.8;

        // --- Subject ---
        if (!string.IsNullOrWhiteSpace(request.Subject))
        {
            (page, gfx, y) = EnsureSpace(document, page, gfx, y, pageHeight, boldFont);
            y = DrawWrappedText(gfx, $"Subject: {request.Subject}", boldFont, MarginLeft, y, usableWidth);
            y += boldFont.GetHeight() * 0.8;
        }

        // --- Salutation ---
        if (!string.IsNullOrWhiteSpace(request.Salutation))
        {
            (page, gfx, y) = EnsureSpace(document, page, gfx, y, pageHeight, regularFont);
            y = DrawWrappedText(gfx, request.Salutation, regularFont, MarginLeft, y, usableWidth);
            y += regularFont.GetHeight() * 0.8;
        }

        // --- Body paragraphs ---
        var paragraphs = (request.Body ?? "").Replace("\r\n", "\n").Split("\n\n");
        foreach (var rawParagraph in paragraphs)
        {
            var paragraph = rawParagraph.Trim();
            if (paragraph.Length == 0) { y += regularFont.GetHeight() * LineSpacing; continue; }

            (page, gfx, y) = EnsureSpace(document, page, gfx, y, pageHeight, regularFont);
            y = DrawWrappedText(gfx, paragraph, regularFont, MarginLeft, y, usableWidth,
                pageHeightLimit: pageHeight - MarginBottom,
                onNewPageNeeded: () =>
                {
                    var np = document.AddPage();
                    var ng = XGraphics.FromPdfPage(np);
                    page = np;
                    gfx = ng;
                    return (np, ng, (double)MarginTop);
                });
            y += regularFont.GetHeight() * 0.8;
        }

        // --- Closing ---
        if (!string.IsNullOrWhiteSpace(request.Closing))
        {
            (page, gfx, y) = EnsureSpace(document, page, gfx, y, pageHeight, regularFont);
            y = DrawWrappedText(gfx, request.Closing, regularFont, MarginLeft, y, usableWidth);
            y += regularFont.GetHeight() * 0.8;
        }

        // --- Signature block ---
        (page, gfx, y) = WriteBlock(document, page, gfx, y, pageHeight, regularFont,
            request.SignOff, request.SignatureName, request.SignatureLine2);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static (PdfPage, XGraphics, double) WriteBlock(
        PdfDocument document, PdfPage page, XGraphics gfx, double y, double pageHeight,
        XFont font, params string[] lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            (page, gfx, y) = EnsureSpace(document, page, gfx, y, pageHeight, font);
            gfx.DrawString(line, font, XBrushes.Black,
                new XRect(MarginLeft, y, page.Width - MarginLeft - MarginRight, font.GetHeight() * LineSpacing),
                XStringFormats.TopLeft);
            y += font.GetHeight() * LineSpacing;
        }
        return (page, gfx, y);
    }

    private static (PdfPage page, XGraphics gfx, double y) EnsureSpace(
        PdfDocument document, PdfPage page, XGraphics gfx, double y, double pageHeight, XFont font)
    {
        if (y + font.GetHeight() * LineSpacing > pageHeight - MarginBottom)
        {
            var newPage = document.AddPage();
            var newGfx = XGraphics.FromPdfPage(newPage);
            return (newPage, newGfx, MarginTop);
        }
        return (page, gfx, y);
    }

    private static double DrawWrappedText(
        XGraphics gfx, string text, XFont font, double x, double y, double maxWidth,
        double? pageHeightLimit = null,
        Func<(PdfPage, XGraphics, double)>? onNewPageNeeded = null)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var line = "";
        double lineHeight = font.GetHeight() * LineSpacing;

        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(line) ? word : $"{line} {word}";
            var size = gfx.MeasureString(candidate, font);

            if (size.Width > maxWidth && !string.IsNullOrEmpty(line))
            {
                gfx.DrawString(line, font, XBrushes.Black, new XRect(x, y, maxWidth, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;

                if (pageHeightLimit.HasValue && y > pageHeightLimit.Value && onNewPageNeeded != null)
                {
                    var (_, newGfx, newY) = onNewPageNeeded();
                    gfx = newGfx;
                    y = newY;
                }

                line = word;
            }
            else
            {
                line = candidate;
            }
        }

        if (!string.IsNullOrEmpty(line))
        {
            gfx.DrawString(line, font, XBrushes.Black, new XRect(x, y, maxWidth, lineHeight), XStringFormats.TopLeft);
            y += lineHeight;
        }

        return y;
    }
}
