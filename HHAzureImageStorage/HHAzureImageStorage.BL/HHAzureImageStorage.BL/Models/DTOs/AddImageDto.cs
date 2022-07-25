using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using Microsoft.Extensions.Logging;
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

        public long SizeInBytes { get; private set; }

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
            int.TryParse(Environment.GetEnvironmentVariable("SHORTEST_PIXEL_SIZE"), out int shortestPixelSize);

            var imageData = ImageSharpResizer.ProcessUploadedImageAndGetData(content, shortestPixelSize, contentType);

            addImageDto.HeightPixels = imageData.HeightPixels;
            addImageDto.WidthPixels = imageData.WidthPixels;
            addImageDto.SizeInBytes = imageData.SizeInBytes;
            addImageDto.Content = imageData.ImageStream;

            logger.LogInformation($"AddImage: Started Resize for MagickImage {imageId} imageId");

            try
            {
                logger.LogInformation($"AddImageDto: Started new MagickImage");

                addImageDto.HasTransparentAlphaLayer = ImageResizerMagickImage
                    .HasImageTransparentAlphaLayer(imageData.ImageStreamForMagicImage == null ? content : imageData.ImageStreamForMagicImage);

                logger.LogInformation($"AddImageDto: Created MagickImage");
            }
            catch (Exception ex)
            {
                logger.LogError($"AddImageDto: Failed first call magickImage.ToByteArray(). Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
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
