using Microsoft.Azure.Cosmos;

namespace HHAzureImageStorage.CosmosRepository.Interfaces
{
    public interface ICosmosContext
    {
        Container Container { get; }
    }
}
