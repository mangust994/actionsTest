using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Tests.Repositories
{
    public class InMemoryImageStorageAccessUrlRepository : IImageStorageAccessUrlRepository
    {
        readonly List<ImageStorageAccessUrl> _collection;

        public InMemoryImageStorageAccessUrlRepository()
        {
            _collection = new List<ImageStorageAccessUrl>();
        }

        public Task<ImageStorageAccessUrl> AddAsync(ImageStorageAccessUrl entity)
        {
            if (entity == null)
            {
                return Task.FromResult<ImageStorageAccessUrl>(null);
            }

            _collection.Add(entity);

            return Task.FromResult(entity);
        }

        public ImageStorageAccessUrl GetByImageIdAndImageVariant(Guid id, ImageVariant imageVariant)
        {
            var item = _collection.FirstOrDefault(x => x.imageId == id);

            return item;
        }

        public Task<ImageStorageAccessUrl> RemoveAsync(Guid id, ImageVariant imageVariant)
        {
            var item = _collection.FirstOrDefault(x => x.imageId == id);

            if (item != null && _collection.Remove(item))
            {
                return Task.FromResult<ImageStorageAccessUrl>(null);
            }

            return Task.FromResult(item);
        }

        public Task<ImageStorageAccessUrl> UpdateAsync(ImageStorageAccessUrl entity)
        {
            var item = GetByImageIdAndImageVariant(entity.imageId, entity.imageVariantId);

            if (item != null)
            {
                item.SaSUrl = entity.SaSUrl;
                item.SasUrlExpireDatetime = entity.SasUrlExpireDatetime;
            }

            return Task.FromResult(item);
        }
    }
}
