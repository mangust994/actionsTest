using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.CosmosRepository.Containers
{
    public class ImageCosmosContext : IImageCosmosContext
    {
        public ImageCosmosContext(CosmosSettings cosmosSettings, CosmosClient cosmosClient) => 
            this.Container = cosmosClient.GetContainer(cosmosSettings.DatabaseName, cosmosSettings.ImageContainerName);

        public Container Container { get; }
    }
}
