using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.CosmosRepository.Containers
{
    public class ImageStorageAccessUrlCosmosContext : IImageStorageAccessUrlCosmosContext
    {
        public ImageStorageAccessUrlCosmosContext(CosmosSettings cosmosSettings, CosmosClient cosmosClient) => 
            this.Container = cosmosClient
                .GetContainer(cosmosSettings.DatabaseName, cosmosSettings.ImageStorageAccessUrlContainerName);

        public Container Container { get; }
    }
}
