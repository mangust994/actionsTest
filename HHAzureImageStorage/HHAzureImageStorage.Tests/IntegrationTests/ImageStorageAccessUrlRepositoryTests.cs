using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.CosmosRepository.Containers;
using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Repositories;
using HHAzureImageStorage.CosmosRepository.Settings;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.Tests.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace HHAzureImageStorage.Tests.IntegrationTests
{
    public class ImageStorageAccessUrlRepositoryTests : IDisposable
    {
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger<ImageService>> _logerMock;
        private readonly ImageStorageAccessUrl _item;
        private readonly IImageStorageAccessUrlRepository _repository;

        const string ChangedTestSaSUrl = "ChangedTestSaSUrl";
        const string TestSaSUrl = "TestSaSUrl";
        const string TestGuid = "f8a41011-9559-486f-a813-29b7d9a94290";

        public ImageStorageAccessUrlRepositoryTests()
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

            IImageStorageAccessUrlCosmosContext imageCosmosContext = new ImageStorageAccessUrlCosmosContext(
                cosmosSettings, cosmosClient);

            _repository = new ImageStorageAccessUrlCosmosRepository(
                imageCosmosContext, _mockLoggerFactory.Object);

            _item = GetTestItem();
        }

        private ImageStorageAccessUrl GetTestItem()
        {
            return new ImageStorageAccessUrl()
            {
                imageId = new Guid(TestGuid),
                SaSUrl = TestSaSUrl,
                imageVariantId = ImageVariant.Temp
            };
        }

        [Fact]
        public async Task AddAsync_TestItem_ItemIsAdded()
        {
            var imageStorage = _repository.GetByImageIdAndImageVariant(
                _item.imageId, _item.imageVariantId);

            if (imageStorage != null)
            {
                Assert.NotNull(imageStorage);

                return;
            }

            var response = await _repository.AddAsync(_item);

            Assert.NotNull(response);
            Assert.Equal(_item.imageVariantId, response.imageVariantId);
            Assert.Equal(_item.imageId, response.imageId);
            Assert.Equal(_item.SaSUrl, response.SaSUrl);
        }

        [Fact]
        public async Task AddAsync_Null_ItemIsNotAdded()
        {
            var response = await _repository.AddAsync(null);

            Assert.Null(response);
        }

        [Fact]
        public async Task GetByImageIdAndImageVariant_TestItem_ImageIsCorrect()
        {
            var response = _repository.GetByImageIdAndImageVariant(
                _item.imageId, _item.imageVariantId);

            if (response == null)
            {
                await _repository.AddAsync(_item);

                response = _repository.GetByImageIdAndImageVariant(
                _item.imageId, _item.imageVariantId);
            }

            Assert.NotNull(response);
            Assert.Equal(_item.imageVariantId, response.imageVariantId);
            Assert.Equal(_item.imageId, response.imageId);
            Assert.Equal(_item.SaSUrl, response.SaSUrl);
        }

        [Fact]
        public async Task UpdateAsync_TestImageStudioKeyAndEventKey_ImageIsValid()
        {
            var imageStorage = _repository.GetByImageIdAndImageVariant(
                _item.imageId, _item.imageVariantId);

            if (imageStorage == null)
            {
                await _repository.AddAsync(_item);

                imageStorage = _repository.GetByImageIdAndImageVariant(
                _item.imageId, _item.imageVariantId);
            }

            Assert.NotNull(imageStorage);

            imageStorage.SaSUrl = ChangedTestSaSUrl;

            var response = await _repository.UpdateAsync(imageStorage);

            Assert.NotNull(response);
            Assert.Equal(ChangedTestSaSUrl, response.SaSUrl);

            response.SaSUrl = TestSaSUrl;

            response = await _repository.UpdateAsync(response);

            Assert.NotNull(response);
            Assert.Equal(TestSaSUrl, response.SaSUrl);

            var removedImage = _repository.RemoveAsync(_item.imageId,
                _item.imageVariantId).Result;
            Assert.Null(removedImage);
        }

        [Fact]
        public void Dispose()
        {
            var imageStorage = _repository
                .RemoveAsync(_item.imageId, _item.imageVariantId).Result;

            Assert.Null(imageStorage);
        }
    }
}
