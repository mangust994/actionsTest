using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Tests.Repositories
{
    public class InMemoryImageStorageRepository : IImageStorageRepository
    {
        readonly List<ImageStorage> _collection;

        public InMemoryImageStorageRepository()
        {
            _collection = new List<ImageStorage>();
        }

        public Task<ImageStorage> AddAsync(ImageStorage entity)
        {
            if (entity == null)
            {
                return Task.FromResult<ImageStorage>(null);
            }

            _collection.Add(entity);

            return Task.FromResult(entity);
        }

        public List<ImageStorage> GetByImageId(Guid imageId)
        {
            var items = _collection.Where(x => x.imageId == imageId).ToList();

            return items;
        }

        public ImageStorage GetByImageIdAndImageVariant(Guid id, ImageVariant imageVariant)
        {
            var item = _collection.FirstOrDefault(x => x.imageId == id && x.imageVariantId == imageVariant);

            return item;
        }

        public Task<ImageStorage> RemoveAsync(Guid id, ImageVariant imageVariant)
        {
            var item = _collection.FirstOrDefault(x => x.imageId == id);

            if (item != null && _collection.Remove(item))
            {
                return Task.FromResult<ImageStorage>(null);
            }

            return Task.FromResult(item);
        }

        public Task<ImageStorage> UpdateAsync(ImageStorage entity)
        {
            var item = GetByImageIdAndImageVariant(entity.imageId, entity.imageVariantId);

            if (item != null)
            {
                item.StorageAccount = entity.StorageAccount;
                item.BlobName = entity.BlobName;
                item.Container = entity.Container;
                item.Status = entity.Status;
                item.SizeInBytes = entity.SizeInBytes;
                item.WidthPixels = entity.WidthPixels;
                item.HeightPixels = entity.HeightPixels;
            }

            return Task.FromResult(item);
        }
    }
}
