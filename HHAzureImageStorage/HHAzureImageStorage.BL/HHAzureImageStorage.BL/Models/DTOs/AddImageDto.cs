using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using ImageMagick;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.IO;

namespace HHAzureImageStorage.BL.Models.DTOs
{
    public class AddImageDto
    {
        public Guid ImageId { get; set; }

        public Stream Content { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }

        public bool HasTransparentAlphaLayer { get; set; }

        public bool ColorCorrectLevel { get; set; }

        public string OriginalImageName { get; set; }

        public bool AutoThumbnails { get; set; }

        public string WatermarkImageId { get; set; }

        public string WatermarkMethod { get; set; }

        public int HHIHPhotographerKey { get; set; }

        public int HHIHEventKey { get; set; }

        public Guid BackupImageGUID { get; set; }

        public ImageVariant ImageVariant { get; set; }

        public ImageUploader SourceApplication { get; set; }

        public int WidthPixels { get; private set; }

        public int HeightPixels { get; private set; }

        public int SizeInBytes { get; private set; }

        public static AddImageDto CreateInstance(Guid imageId, Stream content, string contentType, string originalImageName, string fileName, ImageVariant imageVariant, ImageUploader sourceApp)
        {
            var addImageDto = new AddImageDto()
            {
                ImageId = imageId,
                Content = content,
                ContentType = contentType,
                OriginalImageName = originalImageName,
                Name = fileName,
                ImageVariant = imageVariant,
                SourceApplication = sourceApp
            };

            return addImageDto;
        }

        public static void SetImageData(Guid imageId, Stream content, string contentType, ILogger logger, AddImageDto addImageDto)
        {
            byte[] rotatedImageBytes = null;

            SKEncodedImageFormat imageFormat = GetImageFormat(contentType);

            using (var image = SKImage.FromEncodedData(content))
            {
                if (image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Encode(imageFormat, 1000).SaveTo(ms);
                        rotatedImageBytes = ms.ToArray();
                    }
                }
            }

            SKBitmap sourceBitmap = rotatedImageBytes == null ? SKBitmap.Decode(content)
                : SKBitmap.Decode(rotatedImageBytes);

            logger.LogInformation($"AddImageDto: SKImage - {sourceBitmap.Height} Height, {sourceBitmap.Width} Width");

            int shortestPixelSizeForMagicImage = 20;
            int.TryParse(Environment.GetEnvironmentVariable("SHORTEST_PIXEL_SIZE"), out int shortestPixelSize);
            SKFilterQuality quality = SKFilterQuality.High;

            if (IsShortestPixelSizeBigger(sourceBitmap, shortestPixelSize))
            {
                logger.LogInformation($"AddImageDto: IsShortestPixelSizeBigger Started Resize for {imageId} imageId");

                double scaleForImage = Math.Max(shortestPixelSize / (double)sourceBitmap.Width, shortestPixelSize / (double)sourceBitmap.Height);

                int w = (int)(sourceBitmap.Width * scaleForImage);
                int h = (int)(sourceBitmap.Height * scaleForImage);

                using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(w, h), quality);

                logger.LogInformation($"AddImageDto: Started Encode for {imageId} imageId");

                using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
                using SKData dataOfScaledImage = scaledImage.Encode(imageFormat, 1000);

                logger.LogInformation($"AddImageDto: Finished Encode for {imageId} imageId");

                addImageDto.WidthPixels = scaledBitmap.Width;
                addImageDto.HeightPixels = scaledBitmap.Height;

                logger.LogInformation($"AddImageDto: scaledBitmap - {scaledBitmap.Height} Height, {scaledBitmap.Width} Width");

                logger.LogInformation($"AddImageDto: Started get Bytes FromBitmap for {imageId} imageId");

                var scaledImageBytes = dataOfScaledImage.ToArray();

                logger.LogInformation($"AddImageDto: Started get Bytes FromBitmap for {imageId} imageId");

                addImageDto.SizeInBytes = scaledImageBytes.Length;
                addImageDto.Content = new MemoryStream(scaledImageBytes);

                //content.Seek(0L, SeekOrigin.Begin);

                //addImageDto.Content = content;

                logger.LogInformation($"AddImageDto: Finished MemoryStream for {imageId} imageId");
                logger.LogInformation($"AddImageDto: Finished Resize for {imageId} imageId");
            }
            else
            {
                logger.LogInformation($"AddImageDto: Started Encode for {imageId} imageId");

                using SKImage sourceImage = SKImage.FromBitmap(sourceBitmap);
                using SKData dataOfSourceImage = sourceImage.Encode();

                logger.LogInformation($"AddImageDto: Finished Encode for {imageId} imageId");

                addImageDto.WidthPixels = sourceBitmap.Width;
                addImageDto.HeightPixels = sourceBitmap.Height;

                logger.LogInformation($"AddImageDto: Started get Bytes FromBitmap for {imageId} imageId");

                var scaledImageBytes = dataOfSourceImage.ToArray();

                logger.LogInformation($"AddImageDto: Finished get Bytes FromBitmap for {imageId} imageId");

                addImageDto.SizeInBytes = scaledImageBytes.Length;

                content.Seek(0L, SeekOrigin.Begin);
                addImageDto.Content = content;

                logger.LogInformation($"AddImage: Finished MemoryStream for {imageId} imageId");
            }

            logger.LogInformation($"AddImage: Started Resize for MagickImage {imageId} imageId");

            // Resize the image to get MagickImage
            double scaleForMagicImage = Math.Max(shortestPixelSizeForMagicImage / (double)sourceBitmap.Width, shortestPixelSizeForMagicImage / (double)sourceBitmap.Height);

            int wMI = (int)(sourceBitmap.Width * scaleForMagicImage);
            int hMI = (int)(sourceBitmap.Height * scaleForMagicImage);

            using SKBitmap scaledBitmapForMagickImage = sourceBitmap.Resize(new SKImageInfo(wMI, hMI), quality);
            using SKImage scaledImageForMagickImage = SKImage.FromBitmap(scaledBitmapForMagickImage);
            using SKData dataOfScaledImageForMagickImage = scaledImageForMagickImage.Encode();

            logger.LogInformation($"AddImage: Finished Resize for MagickImage {imageId} imageId");

            byte[] imageBytes = null;

            imageBytes = dataOfScaledImageForMagickImage?.ToArray();

            try
            {
                logger.LogInformation($"AddImageDto: Started new MagickImage");

                MagickImage magickImage = imageBytes == null ?
                    new MagickImage(content)
                    : new MagickImage(imageBytes);

                logger.LogInformation($"AddImageDto: Created MagickImage");

                magickImage.AutoOrient();
                magickImage.RemoveProfile("exif");

                addImageDto.HasTransparentAlphaLayer = magickImage.HasAlpha && !magickImage.IsOpaque;

                //addImageDto.WidthPixels = magickImage.Width;
                //addImageDto.HeightPixels = magickImage.Height;
                //addImageDto.SizeInBytes = magickImage.ToByteArray().Length;

                magickImage.Dispose();

                logger.LogInformation($"AddImageDto: Started new AddImageDto");
            }
            catch (Exception ex)
            {
                logger.LogError($"AddImageDto: Failed first call magickImage.ToByteArray(). Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            sourceBitmap.Dispose();
        }

        private static bool IsShortestPixelSizeBigger(SKBitmap sourceBitmap, int shortestPixelSize)
        {
            return shortestPixelSize != 0 && shortestPixelSize < Math.Min(sourceBitmap.Width, sourceBitmap.Height);
        }

        private static SKEncodedImageFormat GetImageFormat(string contentType)
        {
            //("image/jpeg", "image/png", "image/svg+xml");
            switch (contentType)
            {
                case "image/png":
                    return SKEncodedImageFormat.Png;
                case "image/jpg":
                    return SKEncodedImageFormat.Jpeg;
                default:
                    return SKEncodedImageFormat.Jpeg;
            }
        }

        public Image CreateImageEntity() => new()
        {
            id = ImageId,
            HasTransparentAlphaLayer = HasTransparentAlphaLayer,
            ColorCorrectLevel = ColorCorrectLevel,
            MimeType = ContentType,
            OriginalImageName = OriginalImageName,
            AutoThumbnails = AutoThumbnails,
            WatermarkImageId = WatermarkImageId,
            WatermarkMethod = WatermarkMethod,
            hhihPhotographerKey = HHIHPhotographerKey,
            hhihEventKey = HHIHEventKey,
            BackupImageGUID = BackupImageGUID,
            WidthPixels = WidthPixels,
            HeightPixels = HeightPixels,
            SizeInBytes = SizeInBytes
        };

        public ImageStorage CreateImageStorageEntity(string storageAccountName, string bloabContainerName) => new()
        {
            id = Guid.NewGuid(),
            imageId = this.ImageId,
            imageVariantId = this.ImageVariant,
            Status = ImageStatus.InProgress,
            StorageAccount = storageAccountName,
            Container = bloabContainerName,
            CreatedDate = DateTime.UtcNow,
            BlobName = Name,
            WidthPixels = WidthPixels,
            HeightPixels = HeightPixels,
            SizeInBytes = SizeInBytes
        };

        public ImageApplicationRetention CreateImageApplicationRetentionEntity() => new()
        {
            id = this.ImageId,
            sourceApplicationName = this.SourceApplication.ToString(),
            sourceApplicationReferenceId = null,
            expirationDate = null,
        };
    }
}
