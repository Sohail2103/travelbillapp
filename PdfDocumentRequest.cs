namespace TravelBillApp.Models;

public class PdfDocumentRequest
{
    public string FileName { get; set; } = "document";

    // Letter header
    public string ToName { get; set; } = "";        // e.g. "The Immigration Officer / FRRO"
    public string ToLine2 { get; set; } = "";        // e.g. "India"
    public string FromName { get; set; } = "";
    public string FromLine2 { get; set; } = "";      // e.g. "Passport No: A59584991"

    public string Subject { get; set; } = "";
    public string Salutation { get; set; } = "Respected Sir/Madam,";

    // Main content — separate paragraphs with a blank line
    public string Body { get; set; } = "";

    public string Closing { get; set; } = "Thank you.";
    public string SignOff { get; set; } = "Yours faithfully,";
    public string SignatureName { get; set; } = "";
    public string SignatureLine2 { get; set; } = ""; // e.g. "Passport No: A59584991"
}
