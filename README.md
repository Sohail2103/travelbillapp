# Travel Bill Generator (.NET Web App)

A minimal ASP.NET Core (.NET 8) web application that generates a branded travel invoice / ticket receipt as a **JPEG image**, using SkiaSharp for rendering (server-side, no browser canvas needed).

## Run it

```bash
cd TravelBillApp
dotnet restore
dotnet run
```

> Targets .NET 10 and references the Windows native SkiaSharp assets (`SkiaSharp.NativeAssets.Win32`). If you're deploying to Linux instead, swap that package reference for `SkiaSharp.NativeAssets.Linux` in `TravelBillApp.csproj`.

Then open the URL shown in the console (usually `http://localhost:5000` or `https://localhost:5001`).

Fill the form on the left, click **Generate JPEG Bill**, and the rendered invoice appears on the right with a **Download JPEG** button.

## How it works

- `wwwroot/index.html` — the form UI (navy/gold theme). Submits the form as JSON to the API.
- `Program.cs` — minimal API host. `POST /api/generate-bill` accepts the bill details and returns a JPEG file.
- `Services/BillImageGenerator.cs` — draws the invoice (header, passenger details, flight route, fare table, total, footer) onto a 1200×1600 canvas using SkiaSharp and encodes it as JPEG (quality 92).
- `Models/BillRequest.cs` — the data shape: agency, invoice no/date, passenger, PNR, airline, flight no, route, travel date/time, currency, base fare, taxes, total, notes.

## Customizing

- **Branding colors**: edit the `Navy` / `Gold` / etc. `SKColor` fields at the top of `BillImageGenerator.cs`.
- **Layout / fields**: the generator lays things out top-to-bottom by tracking a `y` cursor — add new `DrawLabelValue` calls the same way to add fields.
- **Multiple agencies** (e.g. Kamal Travels, Thalaivar Deva Travels, Arockiyam Travels): the `AgencyName` field is already free-text, so no code change needed — just type the agency name in the form, or set it as the default in `Models/BillRequest.cs`.
- **Multi-currency** (MYR, INR, USD, SGD): already in the `Currency` dropdown.
- **Deploying to Linux** (e.g. a VPS or container): the `SkiaSharp.NativeAssets.Linux` package is already referenced so text/image rendering works without extra native dependencies.

## Notes

- This was generated without a local .NET SDK to compile against, so give it a `dotnet build` once you have it — if any SkiaSharp API signature has drifted between versions, the fix is almost always a one-line change in `BillImageGenerator.cs`.
- If you want a Tamil-language variant (matching your other receipt formats), swap the `SKTypeface.FromFamilyName("Arial", ...)` calls for a Tamil-supporting font (e.g. "Noto Sans Tamil") and pass Tamil label strings — the rest of the layout logic stays the same.
