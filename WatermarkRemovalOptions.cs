namespace TravelBillApp.Models;

public class WatermarkRemovalOptions
{
    // Height in points (1/72 inch) of the strip at the bottom of each page to
    // white out. WPS Office's "Edit with WPS Office" logo typically sits within
    // about 70-100pt of the bottom edge on an A4/Letter page.
    public double BottomStripHeight { get; set; } = 90;
}
