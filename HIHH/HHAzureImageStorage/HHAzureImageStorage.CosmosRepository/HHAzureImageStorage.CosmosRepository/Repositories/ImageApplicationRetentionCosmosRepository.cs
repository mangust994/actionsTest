using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageApplicationRetentionCosmosRepository : IImageApplicationRetentionRepository
    {
        private readonly IImageApplicationRetentionCosmosContext _context;

        public ImageApplicationRetentionCosmosRepository(IImageApplicationRetentionCosmosContext context) => this._context = context;

        public async Task<ImageApplicationRetention> AddAsync(ImageApplicationRetention entity)
        {
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

            return recordToDelete;
        }

        public async Task<ImageApplicationRetention> UpdateAsync(ImageApplicationRetention entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.id.ToString());

            return await this._context.Container.UpsertItemAsync<ImageApplicationRetention>(entity, partitionKey);
        }
    }
}
