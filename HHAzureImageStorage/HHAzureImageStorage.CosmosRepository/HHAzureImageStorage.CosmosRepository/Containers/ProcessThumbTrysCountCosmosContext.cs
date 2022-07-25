using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.CosmosRepository.Containers
{
    public class ProcessThumbTrysCountCosmosContext : IProcessThumbTrysCountCosmosContext
    {
        public ProcessThumbTrysCountCosmosContext(CosmosSettings cosmosSettings, CosmosClient cosmosClient) =>
            this.Container = cosmosClient.GetContainer(cosmosSettings.DatabaseName, cosmosSettings.ProcessThumbTrysCountContainerName);

        public Container Container { get; }
    }
}
