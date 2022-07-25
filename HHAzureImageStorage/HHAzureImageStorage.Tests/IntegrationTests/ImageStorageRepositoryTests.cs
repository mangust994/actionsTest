using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.Tests.Repositories;

namespace HHAzureImageStorage.Tests.IntegrationTests
{
    public class ImageStorageRepositoryTests
    {
        //private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        //private readonly Mock<ILogger<ImageService>> _logerMock;
        private readonly ImageStorage _item;
        private readonly IImageStorageRepository _repository;

        const string ChangedTestBlobName = "ChangedTestBlobName";
        const string TestStorageAccount = "TestStorageAccount";
        const string TestContainer = "TestContainer";
        const string TestBlobName = "TestBlobName";
        const string TestGuid = "f8a41011-9559-486f-a813-29b7d9a94290";

        public ImageStorageRepositoryTests()
        {
            //_logerMock = new Mock<ILogger<ImageService>>();

            //_logerMock.Setup(x => x.Log(
            //            It.IsAny<LogLevel>(),
            //            It.IsAny<EventId>(),
            //            It.IsAny<It.IsAnyType>(),
            //            It.IsAny<Exception>(),
            //            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            //_logerMock.Setup(m => m.IsEnabled(LogLevel.Information))
            //    .Returns(true);

            //_mockLoggerFactory = new Mock<ILoggerFactory>();
            //_mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            //    .Returns(() => _logerMock.Object);

            //CosmosSettings cosmosSettings = CosmosSettingsExtension.GetTestCosmosSettings();

            //CosmosClient cosmosClient = new CosmosClient(cosmosSettings.EndPoint,
            //    cosmosSettings.Key);

            //IImageStorageCosmosContext imageCosmosContext = new ImageStorageCosmosContext(
            //    cosmosSettings, cosmosClient);

            _repository = new InMemoryImageStorageRepository();

            _item = GetTestItem();
        }

        private ImageStorage GetTestItem()
        {
            return new ImageStorage()
            {
                imageId = new Guid(TestGuid),
                WidthPixels = 1,
                HeightPixels = 1,
                SizeInBytes = 1,
                imageVariantId = ImageVariant.Temp,
                Status = ImageStatus.InProgress,
                StorageAccount = TestStorageAccount,
                Container = TestContainer,
                BlobName = TestBlobName
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
            Assert.Equal(_item.WidthPixels, response.WidthPixels);
            Assert.Equal(_item.HeightPixels, response.HeightPixels);
            Assert.Equal(_item.SizeInBytes, response.SizeInBytes);
            Assert.Equal(_item.Status, response.Status);
            Assert.Equal(_item.StorageAccount, response.StorageAccount);
            Assert.Equal(_item.Container, response.Container);
            Assert.Equal(_item.BlobName, response.BlobName);
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
            Assert.Equal(_item.WidthPixels, response.WidthPixels);
            Assert.Equal(_item.HeightPixels, response.HeightPixels);
            Assert.Equal(_item.SizeInBytes, response.SizeInBytes);
            Assert.Equal(_item.Status, response.Status);
            Assert.Equal(_item.StorageAccount, response.StorageAccount);
            Assert.Equal(_item.Container, response.Container);
            Assert.Equal(_item.BlobName, response.BlobName);
        }

        [Fact]
        public async Task GetByImageId_TestItemImageId_ImageIsValid()
        {
            var response = _repository.GetByImageId(
                _item.imageId);

            if (response.Count == 0)
            {
                await _repository.AddAsync(_item);

                response = _repository.GetByImageId(
                _item.imageId);
            }

            Assert.NotEmpty(response);
            var imageStorage = Assert.Single(response);

            Assert.NotNull(imageStorage);
            Assert.Equal(_item.imageVariantId, imageStorage.imageVariantId);
            Assert.Equal(_item.imageId, imageStorage.imageId);
            Assert.Equal(_item.WidthPixels, imageStorage.WidthPixels);
            Assert.Equal(_item.HeightPixels, imageStorage.HeightPixels);
            Assert.Equal(_item.SizeInBytes, imageStorage.SizeInBytes);
            Assert.Equal(_item.Status, imageStorage.Status);
            Assert.Equal(_item.StorageAccount, imageStorage.StorageAccount);
            Assert.Equal(_item.Container, imageStorage.Container);
            Assert.Equal(_item.BlobName, imageStorage.BlobName);
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

            imageStorage.BlobName = ChangedTestBlobName;

            var response = await _repository.UpdateAsync(imageStorage);

            Assert.NotNull(response);
            Assert.Equal(ChangedTestBlobName, response.BlobName);

            response.BlobName = TestBlobName;

            response = await _repository.UpdateAsync(response);

            Assert.NotNull(response);
            Assert.Equal(TestBlobName, response.BlobName);

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
