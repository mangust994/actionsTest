using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.IntegrationHHIH.Models;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.BL.Services
{
    public interface IUploadImageService
    {
        Task<string> GetAccesUrl(Guid imageIdGuid, ImageVariant imageVariant);
        Task<Image> GetImageAsync(Guid imageId);
        Task<ImageUpload> GetImageUpdateAsync(Guid imageId);
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