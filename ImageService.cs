using ImageScaler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Simple service test method
app.MapGet("/test", () =>
{
    return "Service OK";
})
.WithName("GetTest")
.WithOpenApi();

// Image size scaler with optional change of format
app.MapPost("/ScaleImageSize", async (IFormFile file, int width, int height, string? newFileExt) =>
{
    var chk = CheckFile(file);

    if (chk == null)
    {
        var convertedImage = ImageConversion.ScaleImage(file, width, height, newExt: newFileExt).Result;
        return Results.File(convertedImage.ImageStream.ToArray(), convertedImage.MIMEType, $"resized_image{convertedImage.FileExtension}");
    }

    await Task.CompletedTask;
    return chk;

}).DisableAntiforgery();

// Image percentage scaler with optional change of format
app.MapPost("/ScaleImagePercentage", async (IFormFile file, int percentageChange, string? newFileExt) =>
{
    var chk = CheckFile(file);

    if (chk == null)
    {
        var convertedImage = ImageConversion.ScaleImage(file, percentageChange, newExt: newFileExt).Result;
        return Results.File(convertedImage.ImageStream.ToArray(), convertedImage.MIMEType, $"resized_image{convertedImage.FileExtension}");
    }

    await Task.CompletedTask;
    return chk;

}).DisableAntiforgery();

// Image file format changer (e.g. .jpg to .png)
app.MapPost("/ChangeImageFileType", async (IFormFile file, string newFileExt) =>
{
    var chk = CheckFile(file);

    if (chk == null)
    {
        var convertedImage = ImageConversion.ChangeType(file, newFileExt).Result;
        return Results.File(convertedImage.ImageStream.ToArray(), convertedImage.MIMEType, $"converted_image{convertedImage.FileExtension}");
    }

    await Task.CompletedTask;
    return chk;

}).DisableAntiforgery();

app.Run();

// Checks the file is an image file format and not an empty file
static IResult? CheckFile(IFormFile file)
{
    if (file.ContentType.StartsWith("image"))
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No image uploaded.");

        return null;
    }
    
    return Results.BadRequest("Invalid image file");
}
