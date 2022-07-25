using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;

namespace HHAzureImageStorage.Tests.Repositories
{
    internal class InMemoryProcessThumbTrysCountRepository : IProcessThumbTrysCountRepository
    {
        readonly List<ProcessThumbTrysCount> _collection;

        public InMemoryProcessThumbTrysCountRepository()
        {
            _collection = new List<ProcessThumbTrysCount>();
        }

        public Task<ProcessThumbTrysCount> AddAsync(ProcessThumbTrysCount entity)
        {
            if (entity == null)
            {
                return Task.FromResult<ProcessThumbTrysCount>(null);
            }

            _collection.Add(entity);

            return Task.FromResult(entity);
        }
        
        public Task<ProcessThumbTrysCount> GetByIdAsnc(Guid id)
        {
            var image = _collection.FirstOrDefault(x => x.id == id);

            return Task.FromResult(image);
        }

        public Task<ProcessThumbTrysCount> RemoveAsync(Guid id)
        {
            var image = _collection.FirstOrDefault(x => x.id == id);

            if (image != null && _collection.Remove(image))
            {
                return Task.FromResult<ProcessThumbTrysCount>(null);
            }

            return Task.FromResult(image);
        }

        public Task<ProcessThumbTrysCount> UpdateAsync(ProcessThumbTrysCount entity)
        {
            var image = _collection.FirstOrDefault(x => x.id == entity.id);

            if (image != null)
            {
                image.ReTrysCount = entity.ReTrysCount;
            }

            return Task.FromResult(image);
        }
    }
}
