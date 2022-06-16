using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using ImageMagick;
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
            MagickImage magickImage = new MagickImage(content);

            magickImage.AutoOrient();
            magickImage.RemoveProfile("exif");


            var addImageDto = new AddImageDto()
            {
                ImageId = imageId,
                Content = content,
                ContentType = contentType,
                OriginalImageName = originalImageName,
                Name = fileName,
                ImageVariant = imageVariant,
                SourceApplication = sourceApp,
                WidthPixels = magickImage.Width,
                HeightPixels = magickImage.Height,
                SizeInBytes = magickImage.ToByteArray().Length
            };

            addImageDto.HasTransparentAlphaLayer = magickImage.HasAlpha && !magickImage.IsOpaque;

            int.TryParse(Environment.GetEnvironmentVariable("SHORTEST_PIXEL_SIZE"), out int shortestPixelSize);

            if (Math.Min(magickImage.Width, magickImage.Height) > shortestPixelSize)
            {
                // Resize the image
                double scale = Math.Max(shortestPixelSize / (double)magickImage.Width, shortestPixelSize / (double)magickImage.Height);

                int w = (int)(magickImage.Width * scale);
                int h = (int)(magickImage.Height * scale);

                magickImage.Resize(w, h);

                var buffer = magickImage.ToByteArray();

                addImageDto.WidthPixels = magickImage.Width;
                addImageDto.HeightPixels = magickImage.Height;
                addImageDto.SizeInBytes = buffer.Length;
                addImageDto.Content = new MemoryStream(buffer);
            }           

            return addImageDto;
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
