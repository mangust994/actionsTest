using HHAzureImageStorage.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.DAL.Interfaces
{
    public interface IRepository<T> where T: IEntity
    {
        Task<T> GetByIdAsnc(Guid id);        

        Task<T> AddAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task<T> RemoveAsync(Guid id);
    }
}
