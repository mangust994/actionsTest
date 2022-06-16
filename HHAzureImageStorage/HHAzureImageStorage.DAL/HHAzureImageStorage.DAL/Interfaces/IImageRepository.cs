using HHAzureImageStorage.Domain.Entities;
using System;
using System.Collections.Generic;

namespace HHAzureImageStorage.DAL.Interfaces
{
    public interface IImageRepository : IRepository<Image>
    {
        List<Image> GetByEventKey(int eventKey);
        List<Image> GetByStudioKey(int studioKey);
        List<Image> GetByStudioKeyAndEventKey(int studioKey, int eventKey);
        List<Image> GetByWatermarkIdAndStudioKey(Guid imageId, int studioKey);
    }
}
