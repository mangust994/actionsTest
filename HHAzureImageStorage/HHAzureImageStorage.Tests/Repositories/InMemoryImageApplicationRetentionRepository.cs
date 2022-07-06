using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;

namespace HHAzureImageStorage.Tests.Repositories
{
    public class InMemoryImageApplicationRetentionRepository : IImageApplicationRetentionRepository
    {
        readonly List<ImageApplicationRetention> _collection;

        public InMemoryImageApplicationRetentionRepository()
        {
            _collection = new List<ImageApplicationRetention>();
        }

        public Task<ImageApplicationRetention> AddAsync(ImageApplicationRetention entity)
        {
            if (entity == null)
            {
                return Task.FromResult<ImageApplicationRetention>(null);
            }
            
            _collection.Add(entity);

            return Task.FromResult(entity);
        }

        public Task<ImageApplicationRetention> GetByIdAsnc(Guid id)
        {
            var item = _collection.FirstOrDefault(x => x.id == id);

            return Task.FromResult(item);
        }

        public Task<ImageApplicationRetention> RemoveAsync(Guid id)
        {
            var item = _collection.FirstOrDefault(x => x.id == id);

            if (item != null && _collection.Remove(item))
            {
                return Task.FromResult<ImageApplicationRetention>(null);
            }

            return Task.FromResult(item);
        }

        public async Task<ImageApplicationRetention> UpdateAsync(ImageApplicationRetention entity)
        {
            ImageApplicationRetention? item = _collection.FirstOrDefault(x => x.id == entity.id);

            if (item != null)
            {
                item.sourceApplicationName = entity.sourceApplicationName;
                item.sourceApplicationReferenceId = entity.sourceApplicationReferenceId;
                item.expirationDate = entity.expirationDate;
            }

            return item;
        }
    }
}
