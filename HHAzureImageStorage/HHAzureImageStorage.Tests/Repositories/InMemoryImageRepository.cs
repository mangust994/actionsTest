using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;

namespace HHAzureImageStorage.Tests.Repositories
{
    public class InMemoryImageRepository : IImageRepository
    {
        readonly List<Image> _imageCollection;

        public InMemoryImageRepository()
        {
            _imageCollection = new List<Image>();
        }
        
        public Task<Image> AddAsync(Image entity)
        {
            if (entity == null)
            {
                return Task.FromResult<Image>(null);
            }

            _imageCollection.Add(entity);

            return Task.FromResult(entity);
        }

        public List<Image> GetByEventKey(int eventKey)
        {
            return _imageCollection.Where(x => x.hhihEventKey == eventKey).ToList();
        }

        public Task<Image> GetByIdAsnc(Guid id)
        {
            var image = _imageCollection.FirstOrDefault(x => x.id == id);

            return Task.FromResult(image);
        }

        public List<Image> GetByStudioKey(int studioKey)
        {
            return _imageCollection.Where(x => x.hhihEventKey == studioKey).ToList();
        }

        public List<Image> GetByStudioKeyAndEventKey(int studioKey, int eventKey)
        {
            return _imageCollection.Where(x => x.hhihEventKey == studioKey
                                    && x.hhihEventKey == eventKey).ToList();
        }

        public List<Image> GetByWatermarkIdAndStudioKey(Guid imageId, int studioKey)
        {
            return _imageCollection.Where(x => x.hhihEventKey == studioKey
                                    && x.WatermarkImageId == imageId.ToString())
                                    .ToList();
        }

        public Task<Image> RemoveAsync(Guid id)
        {
            var image = _imageCollection.FirstOrDefault(x => x.id == id);

            if (image != null && _imageCollection.Remove(image))
            {
                return Task.FromResult<Image>(null);
            }

            return Task.FromResult(image);
        }

        public Task<Image> UpdateAsync(Image entity)
        {
            var image = _imageCollection.FirstOrDefault(x => x.id == entity.id);

            if (image != null)
            {
                image.hhihPhotographerKey = entity.hhihPhotographerKey;
                image.hhihEventKey = entity.hhihEventKey;
                image.WidthPixels = entity.WidthPixels;
                image.HeightPixels = entity.HeightPixels;
                image.AutoThumbnails = entity.AutoThumbnails;
                image.BackupImageGUID = entity.BackupImageGUID;
                image.MimeType = entity.MimeType;
                image.SizeInBytes = entity.SizeInBytes;
                image.WatermarkMethod = entity.WatermarkMethod;
                image.WatermarkImageId = entity.WatermarkImageId;
                image.OriginalImageName = entity.OriginalImageName;
            }

            return Task.FromResult(image);
        }
    }
}
