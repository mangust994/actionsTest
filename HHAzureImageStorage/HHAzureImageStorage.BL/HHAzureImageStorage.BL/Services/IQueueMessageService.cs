using HHAzureImageStorage.BL.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.BL.Services
{
    public interface IQueueMessageService
    {
        Task SendMessageDeleteImagesByEventAsync(int eventKey);
        Task SendMessageDeleteImagesByPhotographerAsync(int eventKey);
        Task SendMessageProcessThumbnailImagesAsync(GenerateThumbnailImagesDto data);        
        Task SendMessageRemoveImageAsync(Guid imageId);
        Task SendMessageRebuildThumbnailsWithWatermarkAsync(RebuildThumbnailsWithWatermarkDto modelDto);
    }
}