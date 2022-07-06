using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageStorageAccessUrlCosmosRepository : IImageStorageAccessUrlRepository
    {
        private readonly IImageStorageAccessUrlCosmosContext _context;
        private readonly ILogger _logger;

        public ImageStorageAccessUrlCosmosRepository(IImageStorageAccessUrlCosmosContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ImageStorageAccessUrlCosmosRepository>();
        }

        public async Task<ImageStorageAccessUrl> AddAsync(ImageStorageAccessUrl imageEntity)
        {
            if (imageEntity == null)
            {
                return null;
            }
            
            PartitionKey partitionKey = new PartitionKey(imageEntity.imageId.ToString());

            return await _context.Container.CreateItemAsync(imageEntity, partitionKey);
        }

        public ImageStorageAccessUrl GetByImageIdAndImageVariant(Guid imageId, ImageVariant imageVariant)
        {
            try
            {
                var getImageStorageQuery = _context.Container
                            .GetItemLinqQueryable<ImageStorageAccessUrl>(true);

                return getImageStorageQuery
                    .Where(x => x.imageId == imageId && x.imageVariantId == imageVariant)
                    .ToList().FirstOrDefault();
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"ImageStorageAccessUrlCosmosRepository|GetByImageIdAndImageVariant: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return null;
            }
        }

        public async Task<ImageStorageAccessUrl> RemoveAsync(Guid imageId, ImageVariant imageVariant)
        {
            ImageStorageAccessUrl recordToDelete = this.GetByImageIdAndImageVariant(imageId, imageVariant);

            if (recordToDelete == null)
                return null;

            PartitionKey partitionKey = new PartitionKey(recordToDelete.imageId.ToString());

            return await this._context.Container
                .DeleteItemAsync<ImageStorageAccessUrl>(recordToDelete.id.ToString(), partitionKey);
        }

        public async Task<ImageStorageAccessUrl> UpdateAsync(ImageStorageAccessUrl entity)
        {
            PartitionKey partitionKey = new PartitionKey(entity.imageId.ToString());

            return await this._context.Container.UpsertItemAsync(entity, partitionKey);
        }
    }
}
