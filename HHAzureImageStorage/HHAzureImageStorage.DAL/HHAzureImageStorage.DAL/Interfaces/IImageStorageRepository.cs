using HHAzureImageStorage.Domain.Entities;
using System;
using System.Collections.Generic;

namespace HHAzureImageStorage.DAL.Interfaces
{
    public interface IImageStorageRepository : IRepositoryWithImageVariant<ImageStorage>
    {
        public List<ImageStorage> GetByImageId(Guid imageId);
    }
}
