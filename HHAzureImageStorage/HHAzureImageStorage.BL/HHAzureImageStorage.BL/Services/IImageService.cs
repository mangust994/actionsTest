using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.IntegrationHHIH.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHAzureImageStorage.BL.Services
{
    public interface IImageService
    {
        Task<string> GetAccesUrl(Guid imageIdGuid, ImageVariant imageVariant, int expireInDays);
        List<Image> GetByStudioKey(int studioKey);
        List<Image> GetByWatermarkIdAndStudioKey(Guid imageId, int studioKey);
        Task<Image> GetImageAsync(Guid imageId);
        Task<ImageUpload> GetImageUpdateAsync(Guid imageId);
        Task<string> GetImageUploadSasUrlAsync(GetImageUploadSasUrlDto addImageDto);
        Task<string> ReadAccessUrl(DateTimeOffset expireDateTimeOffset, string bloabName, ImageVariant imageVariant);
        Task RebuildThumbnailsWithWatermarkAsync(RebuildThumbnailsWithWatermarkDto modelDto);
        Task RebuildWatermarkThumbnailsProcess(GenerateThumbnailImagesDto generateThumbnailImagesDto);
        Task RemoveImagesByEventKeyAsync(int eventKey);
        Task RemoveImagesByStudioKeyAsync(int studioKey);
        Task RemoveImageThumbnailsAsync(Guid imageId);
        Task RemoveServiceImageAsync(Guid imageId);        
        Task RemoveUploadedImageAsync(Guid imageId);
        Task RemoveUploadedImageAsync(Guid imageId, string name, ImageVariant imageVariant);
        Task ThumbnailImagesProcess(GenerateThumbnailImagesDto generateThumbnailImagesDto);
        Task UpdateProcessedImagesAsync(AddImageDto addImageDto, AddImageInfoResponseModel hhihAddImageResponse);
        Task UploadImageProcessAsync(AddImageDto addImageDto);
        Task UploadWatermarkImageProcess(AddImageDto addImageDto);
    }
}