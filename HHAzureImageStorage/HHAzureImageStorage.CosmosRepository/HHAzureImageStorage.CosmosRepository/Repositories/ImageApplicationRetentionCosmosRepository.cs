using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageApplicationRetentionCosmosRepository : IImageApplicationRetentionRepository
    {
        private readonly IImageApplicationRetentionCosmosContext _context;
        private readonly ILogger _logger;

        public ImageApplicationRetentionCosmosRepository(IImageApplicationRetentionCosmosContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ImageApplicationRetentionCosmosRepository>();
        }

        public async Task<ImageApplicationRetention> AddAsync(ImageApplicationRetention entity)
        {
            if (entity == null)
            {
                return null;
            }
            
            PartitionKey partitionKey = new PartitionKey(entity.id.ToString());

            ItemResponse<ImageApplicationRetention> itemAsync = await _context
                .Container.CreateItemAsync<ImageApplicationRetention>(entity, partitionKey);

            return itemAsync;
        }

        public async Task<ImageApplicationRetention> GetByIdAsnc(Guid id)
        {
            try
            {
                PartitionKey partitionKey = new PartitionKey(id.ToString());

                return await this._context.Container.ReadItemAsync<ImageApplicationRetention>(id.ToString(), partitionKey);
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"ImageApplicationRetentionCosmosRepository|GetByIdAsnc: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return null;
            }
        }

        public async Task<ImageApplicationRetention> RemoveAsync(Guid id)
        {
            ImageApplicationRetention recordToDelete = await this.GetByIdAsnc(id);

            if (recordToDelete == null)
                return null;

            PartitionKey partitionKey = new PartitionKey(id.ToString());

            ItemResponse<ImageApplicationRetention> itemResponse = await this._context.Container
                .DeleteItemAsync<ImageApplicationRetention>(id.ToString(), partitionKey);

            return itemResponse;
        }

        public async Task<ImageApplicationRetention> UpdateAsync(ImageApplicationRetention entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.id.ToString());

            return await this._context.Container.UpsertItemAsync<ImageApplicationRetention>(entity, partitionKey);
        }
    }
}
