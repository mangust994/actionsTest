using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Tests.UnitTests
{
    public class AddImageDtoTests
    {
        readonly Guid imageId = Guid.NewGuid();
        readonly string contentType = "image/jpeg";
        readonly string originalFileName = "TestOriginalFileName";
        readonly string fileName = "TestFileName";
        readonly string storageAccountName = "TestStorageAccountName";
        readonly string bloabContainerName = "TestBloabContainerName";
        readonly ImageVariant imageVariant = ImageVariant.Temp;
        readonly ImageUploader sourceApp = ImageUploader.UI;

        readonly AddImageDto addImageDto;

        static readonly byte[] emptyImageByteArray = new byte[]
                        {
                            0x47, 0x49, 0x46, 0x38, 0x37, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0x00,
                            0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44, 0x01
                        };

        public AddImageDtoTests()
        {
            addImageDto = CreateInstance();

            addImageDto.HHIHEventKey = 777;
            addImageDto.HHIHPhotographerKey = 1;
            addImageDto.WatermarkImageId = "WatermarkImageId";
            addImageDto.WatermarkMethod = "WatermarkMethod";
            addImageDto.AutoThumbnails = true;
        }

        [Fact]
        public void CreateInstance_IsCorrect()
        {           
            Assert.NotNull(addImageDto);
            Assert.Equal(imageId, addImageDto.ImageId);
            Assert.Equal(contentType, addImageDto.ContentType);
            Assert.Equal(originalFileName, addImageDto.OriginalImageName);
            Assert.Equal(fileName, addImageDto.Name);
            Assert.Equal(imageVariant, addImageDto.ImageVariant);
            Assert.Equal(sourceApp, addImageDto.SourceApplication);
            Assert.NotNull(addImageDto.Content);
            Assert.Equal(1, addImageDto.HeightPixels);
            Assert.Equal(1, addImageDto.WidthPixels);
            Assert.Equal(43, addImageDto.SizeInBytes);
            Assert.False(addImageDto.HasTransparentAlphaLayer);
        }

        [Fact]
        public void CreateImageEntity_IsCorrect()
        {
            Image image = addImageDto.CreateImageEntity();

            Assert.NotNull(image);

            Assert.Equal(image.id, addImageDto.ImageId);
            Assert.Equal(image.HasTransparentAlphaLayer, addImageDto.HasTransparentAlphaLayer);
            Assert.Equal(image.MimeType, addImageDto.ContentType);
            Assert.Equal(image.OriginalImageName, addImageDto.OriginalImageName);
            Assert.Equal(image.AutoThumbnails, addImageDto.AutoThumbnails);
            Assert.Equal(image.WatermarkImageId, addImageDto.WatermarkImageId);
            Assert.Equal(image.WatermarkMethod, addImageDto.WatermarkMethod);
            Assert.Equal(image.hhihPhotographerKey, addImageDto.HHIHPhotographerKey);
            Assert.Equal(image.hhihEventKey, addImageDto.HHIHEventKey);
            Assert.Equal(image.WidthPixels, addImageDto.WidthPixels);
            Assert.Equal(image.HeightPixels, addImageDto.HeightPixels);
            Assert.Equal(image.SizeInBytes, addImageDto.SizeInBytes);
        }

        [Fact]
        public void CreateImageStorageEntity_IsCorrect()
        {
            ImageStorage imageStorage = addImageDto.CreateImageStorageEntity(storageAccountName, bloabContainerName);

            Assert.NotNull(imageStorage);

            Assert.Equal(addImageDto.ImageId, imageStorage.imageId);
            Assert.Equal(addImageDto.ImageVariant, imageStorage.imageVariantId);
            Assert.Equal(ImageStatus.InProgress, imageStorage.Status);
            Assert.Equal(storageAccountName, imageStorage.StorageAccount);
            Assert.Equal(bloabContainerName, imageStorage.Container);
            Assert.Equal(addImageDto.Name, imageStorage.BlobName);
            Assert.Equal(addImageDto.WidthPixels, imageStorage.WidthPixels);
            Assert.Equal(addImageDto.HeightPixels, imageStorage.HeightPixels);
            Assert.Equal(addImageDto.SizeInBytes, imageStorage.SizeInBytes);
        }

        [Fact]
        public void CreateImageApplicationRetentionEntity_IsCorrect()
        {
            ImageApplicationRetention imageApplicationRetention = addImageDto.CreateImageApplicationRetentionEntity();

            Assert.NotNull(imageApplicationRetention);

            Assert.Equal(addImageDto.ImageId, imageApplicationRetention.id);
            Assert.Equal(addImageDto.SourceApplication.ToString(), imageApplicationRetention.sourceApplicationName);
        }

        private AddImageDto CreateInstance()
        {
            AddImageDto addImageDto;

            using (MemoryStream fileStream = new MemoryStream(emptyImageByteArray))
            {
                addImageDto = AddImageDto.CreateInstance(imageId,
                            fileStream, contentType, originalFileName, fileName, imageVariant, sourceApp);
            }

            return addImageDto;
        }
    }
}
