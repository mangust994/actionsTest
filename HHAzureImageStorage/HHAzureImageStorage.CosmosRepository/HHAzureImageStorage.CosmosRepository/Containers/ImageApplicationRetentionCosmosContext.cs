using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.CosmosRepository.Containers
{
    public class ImageApplicationRetentionCosmosContext : IImageApplicationRetentionCosmosContext
    {
        public ImageApplicationRetentionCosmosContext(CosmosSettings cosmosSettings, CosmosClient cosmosClient) =>
            this.Container = cosmosClient.GetContainer(cosmosSettings.DatabaseName, cosmosSettings.ImageApplicationRetentionContainerName);

        public Container Container { get; }
    }
}
