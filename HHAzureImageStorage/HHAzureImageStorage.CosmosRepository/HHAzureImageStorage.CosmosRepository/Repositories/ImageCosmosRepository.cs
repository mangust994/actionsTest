using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HHAzureImageStorage.CosmosRepository.Repositories
{
    public class ImageCosmosRepository : IImageRepository
    {
        private readonly IImageCosmosContext _context;
        private readonly ILogger _logger;

        public ImageCosmosRepository(IImageCosmosContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ImageCosmosRepository>();
        }

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
                _logger.LogError($"ImageCosmosRepository|GetByIdAsnc: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

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
                _logger.LogError($"ImageCosmosRepository|GetByEventKey: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

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
                _logger.LogError($"ImageCosmosRepository|GetByStudioKey: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return null;
            }
        }

        public List<Image> GetByStudioKeyAndEventKey(int studioKey, int eventKey)
        {
            try
            {
                var getImageQuery = _context.Container
                            .GetItemLinqQueryable<Image>(true);

                return getImageQuery
                    .Where(x => x.hhihPhotographerKey == studioKey && x.hhihEventKey == eventKey)
                    .ToList();
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"ImageCosmosRepository|GetByStudioKeyAndEventKey: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

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
                _logger.LogError($"ImageCosmosRepository|GetByWatermarkIdAndStudioKey: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

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
