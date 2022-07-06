using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Tests.Repositories;

namespace HHAzureImageStorage.Tests.IntegrationTests
{
    public class ImageApplicationRetentionRepositoryTests : IDisposable
    {
        //private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        //private readonly Mock<ILogger<ImageService>> _logerMock;
        private readonly ImageApplicationRetention _testItem;
        private readonly IImageApplicationRetentionRepository _storageRepository;

        const string ChangedTestApplicationName = "ChangedTestApplicationName";        
        const string TestApplicationName = "TestApplicationName";
        const string TestGuid = "f8a41011-9559-486f-a813-29b7d9a94290";

        public ImageApplicationRetentionRepositoryTests()
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

            //IImageApplicationRetentionCosmosContext imageCosmosContext = new ImageApplicationRetentionCosmosContext(
            //    cosmosSettings, cosmosClient);

            _storageRepository = new InMemoryImageApplicationRetentionRepository();

            _testItem = GetTestItem();
        }

        private ImageApplicationRetention GetTestItem()
        {
            return new ImageApplicationRetention()
            {
                id = new Guid(TestGuid),
                sourceApplicationName = TestApplicationName
            };
        }

        [Fact]
        public async Task AddAsync_TestItem_ItemIsAdded()
        {
            var item = await _storageRepository.GetByIdAsnc(_testItem.id);

            if (item != null)
            {
                Assert.NotNull(item);

                return;
            }

            var response = await _storageRepository.AddAsync(_testItem);

            Assert.NotNull(response);
            Assert.Equal(_testItem.id, response.id);
            Assert.Equal(_testItem.sourceApplicationName, response.sourceApplicationName);
        }

        [Fact]
        public async Task AddAsync_Null_ItemIsNotAdded()
        {
            var response = await _storageRepository.AddAsync(null);

            Assert.Null(response);
        }

        [Fact]
        public async Task GetByImageId_TestItem_ItemIsCorrect()
        {
            var item = await _storageRepository.GetByIdAsnc(_testItem.id);

            if (item == null)
            {
                await _storageRepository.AddAsync(_testItem);

                item = await _storageRepository.GetByIdAsnc(_testItem.id);
            }

            Assert.NotNull(item);
            Assert.Equal(_testItem.id, item.id);
            Assert.Equal(_testItem.sourceApplicationName, item.sourceApplicationName);
        }

        [Fact]
        public async Task UpdateAsync_TestImageStudioKeyAndEventKey_ImageIsValid()
        {
            var item = await _storageRepository.GetByIdAsnc(_testItem.id);

            if (item == null)
            {
                await _storageRepository.AddAsync(_testItem);

                item = await _storageRepository.GetByIdAsnc(_testItem.id);
            }

            Assert.NotNull(item);

            item.sourceApplicationName = ChangedTestApplicationName;

            var response = await _storageRepository.UpdateAsync(item);

            Assert.NotNull(response);
            Assert.Equal(ChangedTestApplicationName, response.sourceApplicationName);

            response.sourceApplicationName = TestApplicationName;

            response = await _storageRepository.UpdateAsync(response);

            Assert.NotNull(response);
            Assert.Equal(TestApplicationName, response.sourceApplicationName);
        }

        public void Dispose()
        {
            var imageStorage = _storageRepository.RemoveAsync(_testItem.id).Result;

            Assert.Null(imageStorage);
        }
    }
}
