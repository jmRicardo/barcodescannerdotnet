using System.Drawing;
using ZXing;
using ZXing.Common;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/read-barcode", (HttpContext context) =>
{
    var file = context.Request.Form.Files["image"];
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("Image file is missing or empty.");
    }

    using var stream = file.OpenReadStream();
    using var bitmap = new Bitmap(stream);

    var reader = new BarcodeReaderGeneric
    {
        Options = new DecodingOptions
        {
            PossibleFormats = new[] { BarcodeFormat.PDF_417 }
        }
    };

    var lum = new BitmapLuminanceSource(bitmap);

    var result = reader.Decode(lum);
    if (result != null)
    {
        return Results.Ok(result.Text);
    }

    return Results.BadRequest("Failed to decode the barcode.");
});

app.Run();

app.Run();