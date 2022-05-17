using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.CosmosRepository.Containers
{
    public class ImageStorageCosmosContext : IImageStorageCosmosContext
    {
        public ImageStorageCosmosContext(CosmosSettings cosmosSettings, CosmosClient cosmosClient) => 
            this.Container = cosmosClient.GetContainer(cosmosSettings.DatabaseName, cosmosSettings.ImageStorageContainerName);

        public Container Container { get; }
    }
}
