using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Tests.Repositories;

namespace HHAzureImageStorage.Tests.IntegrationTests
{
    public class ImageStorageSizeRepositotyTests
    {
        private readonly IImageStorageSizeRepositoty _storageRepository;

        public ImageStorageSizeRepositotyTests()
        {
            //CosmosSettings cosmosSettings = CosmosSettingsExtension.GetTestCosmosSettings();

            //CosmosClient cosmosClient = new CosmosClient(cosmosSettings.EndPoint,
            //    cosmosSettings.Key);

            //IImageStorageSizeCosmosContext imageCosmosContext = new ImageStorageSizeCosmosContext(
            //    cosmosSettings, cosmosClient);

            _storageRepository = new InMemoryImageStorageSizeRepositoty();
        }        

        [Fact]
        public void GetThumbSizes_ResponseIsNotEmpty()
        {
            List<ImageStorageSize>? response = _storageRepository.GetThumbSizes();

            Assert.NotEmpty(response);
            Assert.Equal(6, response.Count);
        }

        [Fact]
        public void GetWatermarkThumbSizes_ResponseIsNotEmpty()
        {
            List<ImageStorageSize>? response = _storageRepository.GetWatermarkThumbSizes();

            Assert.NotEmpty(response);
            Assert.Equal(3, response.Count);
        }
    }
}
