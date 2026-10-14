using TravelBillApp.Models;
using TravelBillApp.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/generate-bill", (BillRequest request) =>
{
    var jpegBytes = BillImageGenerator.Generate(request);
    var fileName = $"travel-bill-{(string.IsNullOrWhiteSpace(request.InvoiceNo) ? "draft" : request.InvoiceNo)}.jpg";
    return Results.File(jpegBytes, "image/jpeg", fileName);
});

app.MapPost("/api/generate-pdf", (PdfDocumentRequest request) =>
{
    var pdfBytes = PdfDocumentGenerator.Generate(request);
    var safeName = string.IsNullOrWhiteSpace(request.FileName) ? "document" : request.FileName;
    return Results.File(pdfBytes, "application/pdf", $"{safeName}.pdf");
});

app.MapPost("/api/remove-watermark", async (HttpRequest httpRequest) =>
{
    if (!httpRequest.HasFormContentType)
        return Results.BadRequest("Expected multipart/form-data with a 'file' field.");

    var form = await httpRequest.ReadFormAsync();
    var file = form.Files["file"];
    if (file is null || file.Length == 0)
        return Results.BadRequest("No PDF file uploaded.");

    double stripHeight = 90;
    if (form.TryGetValue("stripHeight", out var stripHeightValue)
        && double.TryParse(stripHeightValue, out var parsed))
    {
        stripHeight = parsed;
    }

    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);

    var cleanedBytes = WatermarkRemover.RemoveBottomWatermark(
        ms.ToArray(), new WatermarkRemovalOptions { BottomStripHeight = stripHeight });

    var outputName = Path.GetFileNameWithoutExtension(file.FileName) + "-clean.pdf";
    return Results.File(cleanedBytes, "application/pdf", outputName);
});

app.Run();
