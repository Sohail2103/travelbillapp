namespace TravelBillApp.Models;

public class BillRequest
{
    public string AgencyName { get; set; } = "Mohamed Sohail Travels";
    public string InvoiceNo { get; set; } = "";
    public string InvoiceDate { get; set; } = "";
    public string PassengerName { get; set; } = "";
    public string Pnr { get; set; } = "";
    public string Airline { get; set; } = "";
    public string FlightNo { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string TravelDate { get; set; } = "";
    public string TravelTime { get; set; } = "";
    public string Currency { get; set; } = "INR";
    public decimal BaseFare { get; set; }
    public decimal Taxes { get; set; }
    public decimal Total { get; set; }
    public string Notes { get; set; } = "";
}
