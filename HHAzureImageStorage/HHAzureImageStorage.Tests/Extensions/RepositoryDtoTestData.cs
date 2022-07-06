using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.Core.Models.Responses;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.IntegrationHHIH.Models;

namespace HHAzureImageStorage.Tests.Extensions
{
    public class RepositoryDtoTestData
    {
        static readonly byte[] emptyImageByteArray = new byte[]
                        {
                            0x47, 0x49, 0x46, 0x38, 0x37, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0x00,
                            0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44, 0x01
                        };

        //public static Guid imageId = Guid.NewGuid();
        public static string contentType = "image/jpeg";
        public static string originalFileName = "TestOriginalFileName";
        public static string fileName = "TestFileName";
        public static string storageAccountName = "TestStorageAccountName";
        public static string bloabContainerName = "TestBloabContainerName";
        public static ImageVariant imageVariant = ImageVariant.Temp;
        public static ImageUploader sourceApp = ImageUploader.UI;

        public static AddImageDto GetTestAddImageDtoInstance()
        {
            AddImageDto addImageDto;
            Guid imageId = Guid.NewGuid();

            using (MemoryStream fileStream = new MemoryStream(emptyImageByteArray))
            {
                addImageDto = AddImageDto.CreateInstance(imageId,
                            fileStream, contentType, originalFileName,
                            fileName, imageVariant, sourceApp);

                addImageDto.HHIHEventKey = 777;
                addImageDto.HHIHPhotographerKey = 777;
                addImageDto.WatermarkImageId = "WatermarkImageId";
                addImageDto.WatermarkMethod = "WatermarkMethod";
                addImageDto.AutoThumbnails = true;

                //AddImageDto.SetImageData(imageId, fileStream, contentType,
                //        _logerMock.Object, addImageDto);
            }

            return addImageDto;
        }

        public static AddImageInfoResponseModel GetTestAddImageInfoResponseModelInstance()
        {
            AddImageInfoResponseModel hhihAddImageResponse = new AddImageInfoResponseModel
            {
                WatermarkMethod = "TestWatermarkMethod",
                WatermarkImageId = "TestWatermarkImageId",
                HHIHEventKey = 888,
                HHIHPhotographerKey = 888,
                ExpirationDate = DateTime.Now
            };

            return hhihAddImageResponse;
        }

        public static ImageResizeResponse GetTestImageResizeResponseInstance()
        {
            ImageResizeResponse resizeResponse = new ImageResizeResponse
            {
                ResizedStream = new MemoryStream(emptyImageByteArray),
                WidthPixels = 1,
                HeightPixels = 1,
                SizeInBytes = 1
            };

            resizeResponse.ResizedStream.Position = 0;

            return resizeResponse;
        }

        public static Task<byte[]> GetEmptyImageByteArray()
        {
            return Task.FromResult(emptyImageByteArray);
        }
    }
}
