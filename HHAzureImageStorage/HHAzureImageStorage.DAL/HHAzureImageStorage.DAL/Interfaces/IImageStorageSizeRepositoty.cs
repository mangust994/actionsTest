using HHAzureImageStorage.Domain.Entities;
using System.Collections.Generic;

namespace HHAzureImageStorage.DAL.Interfaces
{
    public interface IImageStorageSizeRepositoty
    {
        List<ImageStorageSize> GetThumbSizes();
        List<ImageStorageSize> GetWatermarkThumbSizes();
    }
}
