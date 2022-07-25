using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ProcessThumbTrysCountCosmosRepository : IProcessThumbTrysCountRepository
    {
        private readonly IProcessThumbTrysCountCosmosContext _context;
        private readonly ILogger _logger;

        public ProcessThumbTrysCountCosmosRepository(IProcessThumbTrysCountCosmosContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ProcessThumbTrysCountCosmosRepository>();
        }

        public async Task<ProcessThumbTrysCount> AddAsync(ProcessThumbTrysCount imageEntity)
        {
            if (imageEntity == null)
            {
                return null;
            }

            PartitionKey partitionKey = new PartitionKey(imageEntity.id.ToString());

            ItemResponse<ProcessThumbTrysCount> itemAsync = await _context
                .Container.CreateItemAsync<ProcessThumbTrysCount>(imageEntity, partitionKey);

            return itemAsync;
        }

        public async Task<ProcessThumbTrysCount> GetByIdAsnc(Guid id)
        {
            try
            {
                PartitionKey partitionKey = new PartitionKey(id.ToString());

                return await this._context.Container.ReadItemAsync<ProcessThumbTrysCount>(id.ToString(), partitionKey);
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"ProcessThumbTrysCountCosmosRepository|GetByIdAsnc: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return null;
            }
        }

        public async Task<ProcessThumbTrysCount> RemoveAsync(Guid id)
        {
            ProcessThumbTrysCount recordToDelete = await this.GetByIdAsnc(id);

            if (recordToDelete == null)
                return null;

            PartitionKey partitionKey = new PartitionKey(id.ToString());

            ItemResponse<ProcessThumbTrysCount> itemResponse = await this._context.Container
                .DeleteItemAsync<ProcessThumbTrysCount>(id.ToString(), partitionKey);

            return itemResponse;
        }

        public async Task<ProcessThumbTrysCount> UpdateAsync(ProcessThumbTrysCount entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.id.ToString());

            return await this._context.Container.UpsertItemAsync<ProcessThumbTrysCount>(entity, partitionKey);
        }
    }
}
