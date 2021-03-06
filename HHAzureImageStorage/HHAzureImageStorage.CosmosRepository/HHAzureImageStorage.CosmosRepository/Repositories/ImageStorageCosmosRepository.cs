using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
using System.Threading.Tasks;
using HHAzureImageStorage.Domain.Enums;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageStorageCosmosRepository : IImageStorageRepository
    {
        private readonly IImageStorageCosmosContext _context;
        private readonly ILogger _logger;

        public ImageStorageCosmosRepository(IImageStorageCosmosContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ImageStorageCosmosRepository>();
        }

        public async Task<ImageStorage> AddAsync(ImageStorage imageEntity)
        {
            if (imageEntity == null)
            {
                return null;
            }
            
            PartitionKey partitionKey = new PartitionKey(imageEntity.imageId.ToString());

            ItemResponse<ImageStorage> itemAsync = await _context.Container.CreateItemAsync<ImageStorage>(imageEntity,
                partitionKey);

            return itemAsync;
        }

        public ImageStorage GetByImageIdAndImageVariant(Guid imageId, ImageVariant imageVariant)
        {
            try
            {
                var getImageStorageQuery = _context.Container
                            .GetItemLinqQueryable<ImageStorage>(true);

                return getImageStorageQuery
                    .Where(x => x.imageId == imageId && x.imageVariantId == imageVariant)
                    .ToList().FirstOrDefault();
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"ImageStorageCosmosRepository|GetByImageIdAndImageVariant: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return null;
            }
        }

        public async Task<ImageStorage> RemoveAsync(Guid imageId, ImageVariant imageVariant)
        {
            ImageStorage recordToDelete = this.GetByImageIdAndImageVariant(imageId, imageVariant);

            if (recordToDelete == null)
                return null;

            PartitionKey partitionKey = new PartitionKey(recordToDelete.imageId.ToString());

            return await this._context.Container
                .DeleteItemAsync<ImageStorage>(recordToDelete.id.ToString(), partitionKey);
        }

        public async Task<ImageStorage> UpdateAsync(ImageStorage entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.imageId.ToString());

            return await this._context.Container.UpsertItemAsync(entity, partitionKey);
        }

        public List<ImageStorage> GetByImageId(Guid imageId)
        {
            try
            {
                var getImageStorageQuery = _context.Container
                            .GetItemLinqQueryable<ImageStorage>(true);

                return getImageStorageQuery
                    .Where(x => x.imageId == imageId)
                    .ToList();
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"ImageStorageCosmosRepository|GetByImageId: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return null;
            }
        }
    }
}
