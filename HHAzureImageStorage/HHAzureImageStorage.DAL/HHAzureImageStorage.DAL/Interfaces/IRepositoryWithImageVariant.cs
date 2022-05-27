using HHAzureImageStorage.Domain.Entities;
using System;
using System.Threading.Tasks;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.DAL.Interfaces
{
    public interface IRepositoryWithImageVariant<T>  where T : IEntity
    {
        T GetByImageIdAndImageVariant(Guid id, ImageVariant imageVariant);

        Task<T> AddAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task<T> RemoveAsync(Guid id, ImageVariant imageVariant);
    }
}
