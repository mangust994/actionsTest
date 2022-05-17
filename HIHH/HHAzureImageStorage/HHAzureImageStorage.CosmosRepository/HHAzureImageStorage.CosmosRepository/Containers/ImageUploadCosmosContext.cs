using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.CosmosRepository.Containers
{
    public class ImageUploadCosmosContext : IImageUploadCosmosContext
    {
        public ImageUploadCosmosContext(CosmosSettings cosmosSettings, CosmosClient cosmosClient) =>
            this.Container = cosmosClient.GetContainer(cosmosSettings.DatabaseName, cosmosSettings.ImageUploadContainerName);

        public Container Container { get; }
    }
}
