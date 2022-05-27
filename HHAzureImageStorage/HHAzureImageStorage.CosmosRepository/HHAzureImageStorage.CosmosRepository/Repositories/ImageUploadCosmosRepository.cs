using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageUploadCosmosRepository : IImageUploadRepository
    {
        private readonly IImageUploadCosmosContext _context;

        public ImageUploadCosmosRepository(IImageUploadCosmosContext context) => this._context = context;

        public async Task<ImageUpload> AddAsync(ImageUpload entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.id.ToString());

            ItemResponse<ImageUpload> itemAsync = await _context
                .Container.CreateItemAsync<ImageUpload>(entity, partitionKey);

            return itemAsync;
        }

        public async Task<ImageUpload> GetByIdAsnc(Guid id)
        {
            try
            {
                PartitionKey partitionKey = new PartitionKey(id.ToString());

                return await this._context.Container.ReadItemAsync<ImageUpload>(id.ToString(), partitionKey);
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }

        public async Task<ImageUpload> RemoveAsync(Guid id)
        {
            ImageUpload recordToDelete = await this.GetByIdAsnc(id);

            if (recordToDelete == null)
                return null;

            PartitionKey partitionKey = new PartitionKey(id.ToString());

            ItemResponse<ImageUpload> itemResponse = await this._context.Container
                .DeleteItemAsync<ImageUpload>(id.ToString(), partitionKey);

            return recordToDelete;
        }

        public async Task<ImageUpload> UpdateAsync(ImageUpload entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.id.ToString());

            return await this._context.Container.UpsertItemAsync<ImageUpload>(entity, partitionKey);
        }
    }
}
