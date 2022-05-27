using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageCosmosRepository : IImageRepository
    {
        private readonly IImageCosmosContext _context;

        public ImageCosmosRepository(IImageCosmosContext context) => this._context = context;

        public async Task<Image> AddAsync(Image imageEntity)
        {
            PartitionKey partitionKey = new PartitionKey(imageEntity.id.ToString());

            ItemResponse<Image> itemAsync = await _context
                .Container.CreateItemAsync<Image>(imageEntity, partitionKey);

            return itemAsync;
        }

        public async Task<Image> GetByIdAsnc(Guid id)
        {
            try
            {
                PartitionKey partitionKey = new PartitionKey(id.ToString());

                return await this._context.Container.ReadItemAsync<Image>(id.ToString(), partitionKey);
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }

        public List<Image> GetByEventKey(int eventKey)
        {
            try
            {
                var getImageQuery = _context.Container
                            .GetItemLinqQueryable<Image>(true);

                return getImageQuery
                    .Where(x => x.hhihEventKey == eventKey)
                    .ToList();
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }

        public List<Image> GetByStudioKey(int studioKey)
        {
            try
            {
                var getImageQuery = _context.Container
                            .GetItemLinqQueryable<Image>(true);

                return getImageQuery
                    .Where(x => x.hhihPhotographerKey == studioKey)
                    .ToList();
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }

        public List<Image> GetByWatermarkIdAndStudioKey(Guid imageId, int studioKey)
        {
            try
            {
                var getImageQuery = _context.Container
                            .GetItemLinqQueryable<Image>(true);

                return getImageQuery
                    .Where(x => x.hhihPhotographerKey == studioKey && x.id == imageId)
                    .ToList();
            }
            catch (CosmosException ex)
            {
                return null;
            }
        }

        public async Task<Image> RemoveAsync(Guid id)
        {
            Image recordToDelete = await this.GetByIdAsnc(id);

            if (recordToDelete == null)
                return null;

            PartitionKey partitionKey = new PartitionKey(id.ToString());

            ItemResponse<Image> itemResponse = await this._context.Container
                .DeleteItemAsync<Image>(id.ToString(), partitionKey);

            return recordToDelete;
        }

        public async Task<Image> UpdateAsync(Image entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.id.ToString());

            return await this._context.Container.UpsertItemAsync<Image>(entity, partitionKey);
        }
    }
}
