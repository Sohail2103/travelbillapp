using SkiaSharp;
using TravelBillApp.Models;

namespace TravelBillApp.Services;

public static class BillImageGenerator
{
    // Brand colors
    private static readonly SKColor Navy = new(0x0B, 0x1F, 0x3A);
    private static readonly SKColor Gold = new(0xC9, 0xA2, 0x27);
    private static readonly SKColor TextDark = new(0x22, 0x22, 0x22);
    private static readonly SKColor TextMuted = new(0x6B, 0x6B, 0x6B);
    private static readonly SKColor RowAlt = new(0xF6, 0xF3, 0xE9);
    private static readonly SKColor Border = new(0xE2, 0xDD, 0xCB);

    private const int Width = 1200;
    private const int Height = 1600;

    private static readonly SKTypeface TypefaceBold =
        SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    private static readonly SKTypeface TypefaceRegular =
        SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

    // All text is drawn with the classic SKPaint text API (TextSize / TextAlign /
    // Typeface / DrawText(string,x,y,paint)) instead of SKFont, because the
    // SKFont-based DrawText overloads differ across SkiaSharp versions and can
    // fail to compile. This legacy API has been stable since SkiaSharp 1.x.
    private static SKPaint TextPaint(SKColor color, float size, bool bold,
        SKTextAlign align = SKTextAlign.Left)
    {
        return new SKPaint
        {
            Color = color,
            TextSize = size,
            IsAntialias = true,
            Typeface = bold ? TypefaceBold : TypefaceRegular,
            TextAlign = align,
            Style = SKPaintStyle.Fill
        };
    }

    public static byte[] Generate(BillRequest bill)
    {
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var fillNavy = new SKPaint { Color = Navy, IsAntialias = true, Style = SKPaintStyle.Fill };
        using var fillGold = new SKPaint { Color = Gold, IsAntialias = true, Style = SKPaintStyle.Fill };
        using var fillRowAlt = new SKPaint { Color = RowAlt, IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokeBorder = new SKPaint { Color = Border, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
        using var strokeGoldLine = new SKPaint { Color = Gold, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };

        using var titleWhite = TextPaint(SKColors.White, 40, true);
        using var subGold = TextPaint(Gold, 22, false);
        using var subWhiteRight = TextPaint(SKColors.White, 22, false, SKTextAlign.Right);
        using var sectionNavy = TextPaint(Navy, 24, true);
        using var labelMuted = TextPaint(TextMuted, 18, true);
        using var valueDark = TextPaint(TextDark, 24, false);
        using var valueDarkRight = TextPaint(TextDark, 24, false, SKTextAlign.Right);
        using var routeDark = TextPaint(TextDark, 36, true);
        using var routeDarkRight = TextPaint(TextDark, 36, true, SKTextAlign.Right);
        using var totalLabelWhite = TextPaint(SKColors.White, 22, true);
        using var totalValGoldRight = TextPaint(Gold, 28, true, SKTextAlign.Right);
        using var smallDark = TextPaint(TextDark, 18, false);
        using var footerWhiteCenter = TextPaint(SKColors.White, 22, false, SKTextAlign.Center);

        // Header band
        canvas.DrawRect(new SKRect(0, 0, Width, 214), fillNavy);
        canvas.DrawRect(new SKRect(0, 214, Width, 220), fillGold);

        canvas.DrawText(bill.AgencyName, 60, 100, titleWhite);
        canvas.DrawText("Travel Invoice / Ticket Receipt", 60, 140, subGold);
        canvas.DrawText($"Invoice No: {bill.InvoiceNo}", Width - 60, 100, subWhiteRight);
        canvas.DrawText($"Date: {bill.InvoiceDate}", Width - 60, 140, subWhiteRight);

        float y = 280;

        y = SectionHeader(canvas, "PASSENGER DETAILS", y, sectionNavy, strokeBorder);
        LabelValue(canvas, "Passenger Name", bill.PassengerName, 60, y, labelMuted, valueDark);
        LabelValue(canvas, "PNR", bill.Pnr, 640, y, labelMuted, valueDark);
        y += 70;
        LabelValue(canvas, "Airline", bill.Airline, 60, y, labelMuted, valueDark);
        LabelValue(canvas, "Flight No", bill.FlightNo, 640, y, labelMuted, valueDark);
        y += 90;

        y = SectionHeader(canvas, "FLIGHT DETAILS", y, sectionNavy, strokeBorder);

        float routeY = y + 6;
        canvas.DrawText(bill.From, 60, routeY - 10, routeDark);
        canvas.DrawText(bill.To, Width - 60, routeY - 10, routeDarkRight);
        canvas.DrawLine(280, routeY, Width - 280, routeY, strokeGoldLine);
        canvas.DrawCircle(280, routeY, 6, fillGold);
        canvas.DrawCircle(Width - 280, routeY, 6, fillGold);

        y = routeY + 50;
        LabelValue(canvas, "Travel Date", bill.TravelDate, 60, y, labelMuted, valueDark);
        LabelValue(canvas, "Departure Time", bill.TravelTime, 640, y, labelMuted, valueDark);
        y += 90;

        y = SectionHeader(canvas, "FARE SUMMARY", y, sectionNavy, strokeBorder);

        float tableTop = y;
        float rowHeight = 60;
        string curr = bill.Currency;

        FareRow(canvas, "Base Fare", $"{curr} {bill.BaseFare:N2}", tableTop, rowHeight, false, valueDark, valueDarkRight, fillRowAlt);
        FareRow(canvas, "Taxes & Fees", $"{curr} {bill.Taxes:N2}", tableTop + rowHeight, rowHeight, true, valueDark, valueDarkRight, fillRowAlt);

        float totalY = tableTop + rowHeight * 2;
        canvas.DrawRect(new SKRect(60, totalY, Width - 60, totalY + rowHeight + 10), fillNavy);
        canvas.DrawText("TOTAL AMOUNT", 90, totalY + 45, totalLabelWhite);
        canvas.DrawText($"{curr} {bill.Total:N2}", Width - 90, totalY + 45, totalValGoldRight);

        y = totalY + rowHeight + 40;

        if (!string.IsNullOrWhiteSpace(bill.Notes))
        {
            canvas.DrawText("Notes:", 60, y, labelMuted);
            y += 30;
            canvas.DrawText(bill.Notes, 60, y, smallDark);
            y += 40;
        }

        // Footer
        canvas.DrawRect(new SKRect(0, Height - 90, Width, Height), fillNavy);
        canvas.DrawText("Thank you for booking with us. Safe travels!", Width / 2f, Height - 45, footerWhiteCenter);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 92);
        return data.ToArray();
    }

    private static float SectionHeader(SKCanvas canvas, string label, float y, SKPaint navyText, SKPaint borderPaint)
    {
        canvas.DrawText(label, 60, y, navyText);
        y += 20;
        canvas.DrawLine(60, y, Width - 60, y, borderPaint);
        return y + 40;
    }

    private static void LabelValue(SKCanvas canvas, string label, string value, float x, float y,
        SKPaint labelPaint, SKPaint valuePaint)
    {
        canvas.DrawText(label.ToUpperInvariant(), x, y, labelPaint);
        canvas.DrawText(string.IsNullOrWhiteSpace(value) ? "-" : value, x, y + 32, valuePaint);
    }

    private static void FareRow(SKCanvas canvas, string label, string value, float rowY, float rowHeight,
        bool alt, SKPaint textPaint, SKPaint textPaintRight, SKPaint altPaint)
    {
        if (alt)
        {
            canvas.DrawRect(new SKRect(60, rowY, Width - 60, rowY + rowHeight), altPaint);
        }
        canvas.DrawText(label, 90, rowY + 38, textPaint);
        canvas.DrawText(value, Width - 90, rowY + 38, textPaintRight);
    }
}
