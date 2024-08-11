using System.Drawing;
using System.Drawing.Imaging;
using Azure;
using Azure.AI.DocumentIntelligence;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/*app.MapPost("/read-barcode",  async (HttpContext context) =>
{
    if (context.Request.ContentLength is null or 0)
    {
        return Results.BadRequest("Image file is missing or empty.");
    }
    
    using var memoryStream = new MemoryStream();
    await context.Request.Body.CopyToAsync(memoryStream);
    memoryStream.Position = 0;

    using var bitmap = new Bitmap(memoryStream);

    var reader = new BarcodeReaderGeneric
    {
        Options = new DecodingOptions
        {
            PossibleFormats = new[] { BarcodeFormat.PDF_417 }
        }
    };
    var result = reader.Decode(new RGBLuminanceSource());
    if (result != null)
    {
        return Results.Ok(result.Text);
    }

    return Results.BadRequest("Failed to decode the barcode.");
});*/

app.MapGet("/write-barcode", async context =>
{
    var barcodeWriter = new BarcodeWriterGeneric
    {
        Format = BarcodeFormat.QR_CODE,
        Options = new QrCodeEncodingOptions
        {
            Width = 200,
            Height = 200
        }
    };

    // Genera el código de barras
    var bitMatrix = barcodeWriter.Encode("This is a test barcode.");

    // Convierte BitMatrix a Bitmap
    var width = bitMatrix.Width;
    var height = bitMatrix.Height;
    using var bitmap = new Bitmap(width, height);
    using (var graphics = Graphics.FromImage(bitmap))
    {
        graphics.Clear(Color.White); // Fondo blanco

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = bitMatrix[x, y] ? Color.Black : Color.White;
                bitmap.SetPixel(x, y, color);
            }
        }
        
        var filePath = "path_to_your_file.png";

        bitmap.Save(filePath, ImageFormat.Png);

        await context.Response.WriteAsync($"Barcode saved to {filePath}");
    }
});

app.MapPost("read-dni", async (HttpContext context) =>
{
    if (context.Request.ContentLength is null or 0)
    {
        return Results.BadRequest("Image file is missing or empty.");
    }

    using var memoryStream = new MemoryStream();
    await context.Request.Body.CopyToAsync(memoryStream);
    memoryStream.Position = 0;

    // Convert the image stream to BinaryData
    var binaryData = BinaryData.FromStream(memoryStream);

    var apiKey = "API_KEY";
    var endpoint = "ENDPOINT";

    var credential = new AzureKeyCredential(apiKey);
    var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

    var content = new AnalyzeDocumentContent { Base64Source = binaryData };

    var features = new List<DocumentAnalysisFeature> { DocumentAnalysisFeature.Barcodes, DocumentAnalysisFeature.QueryFields };

    IEnumerable<string> queryFields = new[] { "Documento" };

    Operation<AnalyzeResult> operation =
        await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-idDocument", content, features: features, queryFields: queryFields);

    var result = operation.Value;

    var identityDocument = result.Documents.Single();
    
    if (identityDocument.Fields.TryGetValue("Sex", out var sexfield))
    {
        if (sexfield.Type == DocumentFieldType.String)
        {
            var sex = sexfield.ValueString;
            Console.WriteLine($"Sex: '{sex}', with confidence {sexfield.Confidence}");
        }
    }

    if (identityDocument.Fields.TryGetValue("FirstName", out var firstNameField))
    {
        if (firstNameField.Type == DocumentFieldType.String)
        {
            var firstName = firstNameField.ValueString;
            Console.WriteLine($"First Name: '{firstName}', with confidence {firstNameField.Confidence}");
        }
    }

    if (identityDocument.Fields.TryGetValue("LastName", out var lastNameField))
    {
        if (lastNameField.Type == DocumentFieldType.String)
        {
            var lastName = lastNameField.ValueString;
            Console.WriteLine($"Last Name: '{lastName}', with confidence {lastNameField.Confidence}");
        }
    }
    
    if (identityDocument.Fields.TryGetValue("DateOfBirth", out var dateOfBirthField))
    {
        if (dateOfBirthField.Type == DocumentFieldType.Date)
        {
            var dateOfBirth = dateOfBirthField.ValueDate;
            Console.WriteLine($"Date Of Birth: '{dateOfBirth}', with confidence {dateOfBirthField.Confidence}");
        }
    }
    
    if (identityDocument.Fields.TryGetValue("Documento", out var documentField))
    {
        if (documentField.Type == DocumentFieldType.String)
        {
            var dni = documentField.ValueString;
            Console.WriteLine($"Numero de documento: '{dni}', with confidence {documentField.Confidence}");
        }
    }
    
    return Results.Ok();
});

app.Run();