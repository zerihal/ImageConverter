using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageScaler
{
    public static class ImageConversion
    {
        private const string JpgExt = ".jpg";
        private const string PngExt = ".png";
        private const string BmpExt = ".bmp";
        private const string TiffExt = ".tiff";
        private const string GifExt = ".gif";

        private static string[] PermittedExtensions = [JpgExt, PngExt, BmpExt, TiffExt, GifExt];

        public static async Task<ConvertedImage> ChangeType(IFormFile file, string newExt)
        {
            return await ScaleImage(file, -1, -1, newFileTypeOnly: true, newExt: newExt);
        }

        public static async Task<ConvertedImage> ScaleImage(IFormFile file, int percentage, string? newExt = null)
        {
            return await ScaleImage(file, -1, -1, percentage, newExt: newExt);
        }

        public static async Task<ConvertedImage> ScaleImage(IFormFile file, int newWidth, int newHeight, int percentage = 0, bool newFileTypeOnly = false, string? newExt = null)
        {
            using (var inputStream = file.OpenReadStream())
            {
                using (var image = await Image.LoadAsync(inputStream))
                {
                    if (percentage > 0)
                    {
                        var pMultiplier = (decimal)percentage / 100;
                        newWidth = Convert.ToInt32(Math.Round(image.Width * pMultiplier, 0));
                        newHeight = Convert.ToInt32(Math.Round(image.Height * pMultiplier, 0));
                    }

                    image.Mutate(x => x.Resize(newWidth, newHeight));

                    using (var memoryStream = new MemoryStream())
                    {
                        var extension = (newExt != null && PermittedExtensions.Contains(newExt) ? newExt : GetFileExtension(file)).ToLowerInvariant();
                        var fileType = GetFileType(extension);

                        switch (extension)
                        {
                            case JpgExt:
                                await image.SaveAsJpegAsync(memoryStream);
                                break;

                            case PngExt:
                                await image.SaveAsPngAsync(memoryStream);
                                break;

                            case BmpExt:
                                await image.SaveAsBmpAsync(memoryStream);
                                break;

                            case TiffExt:
                                await image.SaveAsTiffAsync(memoryStream);
                                break;

                            case GifExt:
                                await image.SaveAsGifAsync(memoryStream);
                                break;
                        }

                        return new ConvertedImage(memoryStream, extension, fileType);
                    }
                }
            }          
        }

        private static string GetFileExtension(IFormFile file) => Path.GetExtension(file.FileName);

        private static string GetFileType(string fileExt)
        {
            var ext = fileExt.ToLowerInvariant();

            switch (ext)
            {
                case JpgExt:
                    return "image/jpeg";

                default:
                    return $"image/{ext}";
            }
        }
    }

    public record ConvertedImage(MemoryStream ImageStream, string FileExtension, string MIMEType);
}
