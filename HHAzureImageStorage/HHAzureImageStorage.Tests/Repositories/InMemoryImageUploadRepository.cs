using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;

namespace HHAzureImageStorage.Tests.Repositories
{
    public class InMemoryImageUploadRepository : IImageUploadRepository
    {
        readonly List<ImageUpload> _collection;

        public InMemoryImageUploadRepository()
        {
            _collection = new List<ImageUpload>();
        }

        public Task<ImageUpload> AddAsync(ImageUpload entity)
        {
            if (entity == null)
            {
                return Task.FromResult<ImageUpload>(null);
            }
            
            _collection.Add(entity);

            return Task.FromResult(entity);
        }

        public Task<ImageUpload> GetByIdAsnc(Guid id)
        {
            var item = _collection.FirstOrDefault(x => x.id == id);

            return Task.FromResult(item);
        }

        public Task<ImageUpload> RemoveAsync(Guid id)
        {
            var item = _collection.FirstOrDefault(x => x.id == id);

            if (item != null && _collection.Remove(item))
            {
                return Task.FromResult<ImageUpload>(null);
            }

            return Task.FromResult(item);
        }

        public Task<ImageUpload> UpdateAsync(ImageUpload entity)
        {
            var item = _collection.FirstOrDefault(x => x.id == entity.id);

            if (item != null)
            {
                item.hhihPhotographerKey = entity.hhihPhotographerKey;
                item.hhihEventKey = entity.hhihEventKey;
                item.ColorCorrectLevel = entity.ColorCorrectLevel;
                item.AlbumKey = entity.AlbumKey;
                item.AutoThumbnails = entity.AutoThumbnails;
                item.WatermarkMethod = entity.WatermarkMethod;
                item.WatermarkImageId = entity.WatermarkImageId;
                item.ExpirationDate = entity.ExpirationDate;
                item.HiResDownload = entity.HiResDownload;
                item.FileName = entity.FileName;
                item.HasTransparentAlphaLayer = entity.HasTransparentAlphaLayer;
                item.OriginalImageName = entity.OriginalImageName;
            }

            return Task.FromResult(item);
        }
    }
}
