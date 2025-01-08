using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageScaler
{
    /// <summary>
    /// Image conversion helper functions.
    /// </summary>
    public static class ImageConversion
    {
        private const string JpgExt = ".jpg";
        private const string PngExt = ".png";
        private const string BmpExt = ".bmp";
        private const string TiffExt = ".tiff";
        private const string GifExt = ".gif";

        /// <summary>
        /// Collection of file extensions permitted.
        /// </summary>
        private static string[] PermittedExtensions = [JpgExt, PngExt, BmpExt, TiffExt, GifExt];

        /// <summary>
        /// Changes the file type of the image file, maintaining existing image dimensions.
        /// </summary>
        /// <param name="file">Original image file (form file).</param>
        /// <param name="newExt">New file extension / type.</param>
        /// <returns>Converted image that can be output as a file from the memory stream.</returns>
        public static async Task<ConvertedImage> ChangeType(IFormFile file, string newExt)
        {
            return await ScaleImage(file, -1, -1, newFileTypeOnly: true, newExt: newExt);
        }

        /// <summary>
        /// Rescales the image by the given percentage, with option to change file type.
        /// </summary>
        /// <param name="file">Original image file.</param>
        /// <param name="percentage">Percentage change.</param>
        /// <param name="newExt">New file extension / type (optional).</param>
        /// <returns>Rescaled converted image.</returns>
        public static async Task<ConvertedImage> ScaleImage(IFormFile file, int percentage, string? newExt = null)
        {
            return await ScaleImage(file, -1, -1, percentage, newExt: newExt);
        }

        /// <summary>
        /// Rescales the image to the given size or percentage (optional) or converts the image to a new file type.
        /// </summary>
        /// <param name="file">Original image file.</param>
        /// <param name="newWidth">New height (px).</param>
        /// <param name="newHeight">New width (px).</param>
        /// <param name="percentage">Percentage change (optional).</param>
        /// <param name="newFileTypeOnly">New file type only with no resizing (optional).</param>
        /// <param name="newExt">New file extension / type (optional).</param>
        /// <remarks>
        /// If percentage change is specified then any new width and height passed in will be ignored in favor of the 
        /// percentage change and if new file type only is set to true then any rescaling will be ignored.
        /// </remarks>
        /// <returns>Converted image.</returns>
        public static async Task<ConvertedImage> ScaleImage(IFormFile file, int newWidth, int newHeight, int percentage = 0, bool newFileTypeOnly = false, string? newExt = null)
        {
            using (var inputStream = file.OpenReadStream())
            {
                using (var image = await Image.LoadAsync(inputStream))
                {
                    if (!newFileTypeOnly)
                    {
                        if (percentage > 0)
                        {
                            var pMultiplier = (decimal)percentage / 100;
                            newWidth = Convert.ToInt32(Math.Round(image.Width * pMultiplier, 0));
                            newHeight = Convert.ToInt32(Math.Round(image.Height * pMultiplier, 0));
                        }

                        image.Mutate(x => x.Resize(newWidth, newHeight));
                    }

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

        /// <summary>
        /// Gets the file extension from the form file.
        /// </summary>
        /// <param name="file">Form file.</param>
        /// <returns>File extension.</returns>
        private static string GetFileExtension(IFormFile file) => Path.GetExtension(file.FileName);

        /// <summary>
        /// Gets the file type as MIME type.
        /// </summary>
        /// <param name="fileExt">File extension.</param>
        /// <returns>File MIME type.</returns>
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

    /// <summary>
    /// Converted image (from file).
    /// </summary>
    /// <param name="ImageStream">Image memory stream.</param>
    /// <param name="FileExtension">File extension.</param>
    /// <param name="MIMEType">MIME type.</param>
    public record ConvertedImage(MemoryStream ImageStream, string FileExtension, string MIMEType);
}
