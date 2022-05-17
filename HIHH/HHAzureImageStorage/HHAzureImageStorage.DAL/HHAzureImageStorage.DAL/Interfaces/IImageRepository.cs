using HHAzureImageStorage.Domain.Entities;
using System.Collections.Generic;

namespace HHAzureImageStorage.DAL.Interfaces
{
    public interface IImageRepository : IRepository<Image>
    {
        List<Image> GetByEventKey(int eventKey);
        List<Image> GetByStudioKey(int studioKey);
    }
}
