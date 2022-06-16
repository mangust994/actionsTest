using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.CosmosRepository.Containers;
using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Repositories;
using HHAzureImageStorage.CosmosRepository.Settings;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Tests.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace HHAzureImageStorage.Tests.IntegrationTests
{
    public class ImageRepositoryTests : IDisposable
    {
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger<ImageService>> _logerMock;
        private readonly Image _testImage;
        private readonly IImageRepository _imageRepository;

        const string ChangedName = "ChangedTestName.png";
        const string ChangedMimeType = "image/png";
        const string TestName = "TestName.jpg";
        const string TestMimeType = "image/jpeg";
        const string TestGuid = "f8a41011-9559-486f-a813-29b7d9a94290";

        public ImageRepositoryTests()
        {
            _logerMock = new Mock<ILogger<ImageService>>();

            _logerMock.Setup(x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            _logerMock.Setup(m => m.IsEnabled(LogLevel.Information))
                .Returns(true);

            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(() => _logerMock.Object);

            CosmosSettings cosmosSettings = CosmosSettingsExtension.GetTestCosmosSettings();
            
            CosmosClient cosmosClient = new CosmosClient(cosmosSettings.EndPoint,
                cosmosSettings.Key);

            IImageCosmosContext imageCosmosContext = new ImageCosmosContext(
                cosmosSettings, cosmosClient);

            _imageRepository = new ImageCosmosRepository(
                imageCosmosContext, _mockLoggerFactory.Object);

            _testImage = GetTestImage();
        }

        private Image GetTestImage()
        {
            return new Image()
            {
                AutoThumbnails = true,
                id = new Guid(TestGuid),
                WatermarkImageId = TestGuid,
                WidthPixels = 1,
                HeightPixels = 1,
                SizeInBytes = 1,
                OriginalImageName = TestName,
                MimeType = TestMimeType,
                hhihEventKey = 777,
                hhihPhotographerKey = 777
            };
        }
        
        [Fact]
        public async Task AddAsync_TestImage_ImageIsAdded()
        {
            var image = await _imageRepository.GetByIdAsnc(_testImage.id);

            if (image != null)
            {
                return;
            }

            var response = await _imageRepository.AddAsync(_testImage);

            Assert.NotNull(response);
            Assert.Equal(_testImage.AutoThumbnails, response.AutoThumbnails);
            Assert.Equal(_testImage.id, response.id);
            Assert.Equal(_testImage.WidthPixels, response.WidthPixels);
            Assert.Equal(_testImage.HeightPixels, response.HeightPixels);
            Assert.Equal(_testImage.SizeInBytes, response.SizeInBytes);
            Assert.Equal(_testImage.OriginalImageName, response.OriginalImageName);
            Assert.Equal(_testImage.MimeType, response.MimeType);
        }

        [Fact]
        public async Task AddAsync_Null_ItemIsNotAdded()
        {
            var response = await _imageRepository.AddAsync(null);

            Assert.Null(response);
        }

        [Fact]
        public async Task GetByIdAsnc_TestImageId_ImageIsCorrect()
        {
            var response = await _imageRepository.GetByIdAsnc(_testImage.id);

            if (response == null)
            {
                await _imageRepository.AddAsync(_testImage);

                response = await _imageRepository.GetByIdAsnc(_testImage.id);
            }            

            Assert.NotNull(response);
            Assert.Equal(_testImage.AutoThumbnails, response.AutoThumbnails);
            Assert.Equal(_testImage.id, response.id);
            Assert.Equal(_testImage.WidthPixels, response.WidthPixels);
            Assert.Equal(_testImage.HeightPixels, response.HeightPixels);
            Assert.Equal(_testImage.SizeInBytes, response.SizeInBytes);
            Assert.Equal(_testImage.OriginalImageName, response.OriginalImageName);
            Assert.Equal(_testImage.MimeType, response.MimeType);
            Assert.Equal(_testImage.hhihEventKey, response.hhihEventKey);
            Assert.Equal(_testImage.hhihPhotographerKey, response.hhihPhotographerKey);
        }

        [Fact]
        public async Task GetByEventKey_TestImageEventId_ImageIsValid()
        {
            var response = _imageRepository.GetByEventKey(_testImage.hhihEventKey);

            if (response.Count == 0)
            {
                await _imageRepository.AddAsync(_testImage);

                response = _imageRepository.GetByEventKey(_testImage.hhihEventKey);
            }

            Assert.NotEmpty(response);
            var image = Assert.Single(response);
            
            Assert.Equal(_testImage.AutoThumbnails, image.AutoThumbnails);
            Assert.Equal(_testImage.id, image.id);
            Assert.Equal(_testImage.WidthPixels, image.WidthPixels);
            Assert.Equal(_testImage.HeightPixels, image.HeightPixels);
            Assert.Equal(_testImage.SizeInBytes, image.SizeInBytes);
            Assert.Equal(_testImage.OriginalImageName, image.OriginalImageName);
            Assert.Equal(_testImage.MimeType, image.MimeType);
            Assert.Equal(_testImage.hhihEventKey, image.hhihEventKey);
            Assert.Equal(_testImage.hhihPhotographerKey, image.hhihPhotographerKey);
        }

        [Fact]
        public async Task GetByStudioKey_TestImageStudioId_ImageIsValid()
        {
            var response = _imageRepository.GetByStudioKey(_testImage.hhihPhotographerKey);

            if (response.Count == 0)
            {
                await _imageRepository.AddAsync(_testImage);

                response = _imageRepository.GetByStudioKey(_testImage.hhihPhotographerKey);
            }

            Assert.NotEmpty(response);
            var image = Assert.Single(response);

            Assert.Equal(_testImage.AutoThumbnails, image.AutoThumbnails);
            Assert.Equal(_testImage.id, image.id);
            Assert.Equal(_testImage.WidthPixels, image.WidthPixels);
            Assert.Equal(_testImage.HeightPixels, image.HeightPixels);
            Assert.Equal(_testImage.SizeInBytes, image.SizeInBytes);
            Assert.Equal(_testImage.OriginalImageName, image.OriginalImageName);
            Assert.Equal(_testImage.MimeType, image.MimeType);
            Assert.Equal(_testImage.hhihEventKey, image.hhihEventKey);
            Assert.Equal(_testImage.hhihPhotographerKey, image.hhihPhotographerKey);
        }

        [Fact]
        public async Task GetByStudioKeyAndEventKey_TestImageStudioKeyAndEventKey_ImageIsValid()
        {
            var response = _imageRepository
                .GetByStudioKeyAndEventKey(_testImage.hhihEventKey, _testImage.hhihPhotographerKey);

            if (response.Count == 0)
            {
                await _imageRepository.AddAsync(_testImage);

                response = _imageRepository.
                    GetByStudioKeyAndEventKey(_testImage.hhihEventKey, _testImage.hhihPhotographerKey);
            }

            Assert.NotEmpty(response);
            var image = Assert.Single(response);

            Assert.Equal(_testImage.AutoThumbnails, image.AutoThumbnails);
            Assert.Equal(_testImage.id, image.id);
            Assert.Equal(_testImage.WidthPixels, image.WidthPixels);
            Assert.Equal(_testImage.HeightPixels, image.HeightPixels);
            Assert.Equal(_testImage.SizeInBytes, image.SizeInBytes);
            Assert.Equal(_testImage.OriginalImageName, image.OriginalImageName);
            Assert.Equal(_testImage.MimeType, image.MimeType);
            Assert.Equal(_testImage.hhihEventKey, image.hhihEventKey);
            Assert.Equal(_testImage.hhihPhotographerKey, image.hhihPhotographerKey);
        }

        [Fact]
        public async Task GetByWatermarkIdAndStudioKey_TestImageStudioKeyAndEventKey_ImageIsValid()
        {
            var watermarkImageId = new Guid(TestGuid);
            
            var response = _imageRepository.
                GetByWatermarkIdAndStudioKey(watermarkImageId, _testImage.hhihPhotographerKey);

            if (response.Count == 0)
            {
                await _imageRepository.AddAsync(_testImage);

                response = _imageRepository.
                    GetByWatermarkIdAndStudioKey(watermarkImageId, _testImage.hhihPhotographerKey);
            }

            Assert.NotEmpty(response);
            var image = Assert.Single(response);

            Assert.Equal(_testImage.AutoThumbnails, image.AutoThumbnails);
            Assert.Equal(_testImage.id, image.id);
            Assert.Equal(_testImage.WidthPixels, image.WidthPixels);
            Assert.Equal(_testImage.HeightPixels, image.HeightPixels);
            Assert.Equal(_testImage.SizeInBytes, image.SizeInBytes);
            Assert.Equal(_testImage.OriginalImageName, image.OriginalImageName);
            Assert.Equal(_testImage.MimeType, image.MimeType);
            Assert.Equal(_testImage.hhihEventKey, image.hhihEventKey);
            Assert.Equal(_testImage.hhihPhotographerKey, image.hhihPhotographerKey);
        }

        [Fact]
        public async Task UpdateAsync_TestImageStudioKeyAndEventKey_ImageIsValid()
        {
            var image = await _imageRepository.GetByIdAsnc(_testImage.id);

            if (image == null)
            {
                await _imageRepository.AddAsync(_testImage);

                image = await _imageRepository.GetByIdAsnc(_testImage.id);
            }

            Assert.NotNull(image);

            image.OriginalImageName = ChangedName;
            image.MimeType = ChangedMimeType;

            var response = await _imageRepository.UpdateAsync(image);

            Assert.NotNull(response);
            Assert.Equal(ChangedName, response.OriginalImageName);
            Assert.Equal(ChangedMimeType, response.MimeType);

            response.OriginalImageName = TestName;
            response.MimeType = TestMimeType;

            response = await _imageRepository.UpdateAsync(response);

            Assert.NotNull(response);
            Assert.Equal(TestName, response.OriginalImageName);
            Assert.Equal(TestMimeType, response.MimeType);

            var removedImage = await _imageRepository.RemoveAsync(_testImage.id);
            Assert.Null(removedImage);
        }

        //[Fact]
        //public async Task RemoveAsync_TestImageId_ImageIsRemoved()
        //{
        //    var image = await _imageRepository.GetByIdAsnc(_testImage.id);

        //    if (image == null)
        //    {
        //        await _imageRepository.AddAsync(_testImage);
        //    }

        //    image = await _imageRepository.RemoveAsync(_testImage.id);

        //    Assert.Null(image);
        //}

        //[Fact]
        public void Dispose()
        {
            var image = _imageRepository.RemoveAsync(_testImage.id).Result;

            Assert.Null(image);
        }
    }
}
