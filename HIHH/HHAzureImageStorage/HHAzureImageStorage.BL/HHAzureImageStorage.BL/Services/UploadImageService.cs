using HHAzureImageStorage.BL.Extensions;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.BlobStorageProcessor.Interfaces;
using HHAzureImageStorage.BlobStorageProcessor.Settings;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.IntegrationHHIH.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HHAzureImageStorage.BL.Services
{
    public class UploadImageService : IUploadImageService
    {
        private readonly ILogger _logger;

        private readonly BlobStorageSettings _blobStorageSettings;

        private readonly IImageRepository _imageRepository;
        private readonly IImageStorageRepository _imageStorageRepository;
        private readonly IImageApplicationRetentionRepository _imageApplicationRetentionRepository;

        private readonly IStorageHelper _storageHelper;
        private readonly IStorageProcessor _storageProcessor;
        private readonly IImageResizer _imageResizer;
        private readonly IImageStorageSizeRepositoty _imageStorageSizeRepository;
        private readonly IImageStorageAccessUrlRepository _imageStorageAccesUrlRepository;
        private readonly IImageUploadRepository _imageUploadRepository;


        public UploadImageService(ILoggerFactory loggerFactory,
            BlobStorageSettings blobStorageOptions,
            IImageRepository imageRepository,
                                IImageStorageRepository imageStorageRepository,
                                IImageApplicationRetentionRepository imageApplicationRetentionRepository,
                                IStorageHelper storageHelper,
                                IImageResizer imageResizer,
                                IImageStorageSizeRepositoty imageStorageSizeRepository,
                                IImageStorageAccessUrlRepository imageStorageAccesUrlRepository,
                                IImageUploadRepository imageUploadRepository,
                                IStorageProcessor storageProcessor)
        {
            _logger = loggerFactory.CreateLogger<UploadImageService>();
            _blobStorageSettings = blobStorageOptions;
            _imageRepository = imageRepository;
            _storageHelper = storageHelper;
            _storageProcessor = storageProcessor;
            _imageStorageRepository = imageStorageRepository;
            _imageApplicationRetentionRepository = imageApplicationRetentionRepository;
            _imageResizer = imageResizer;
            _imageStorageSizeRepository = imageStorageSizeRepository;
            _imageStorageAccesUrlRepository = imageStorageAccesUrlRepository;
            _imageUploadRepository = imageUploadRepository;
        }

        public async Task UploadImageProcessAsync(AddImageDto addImageDto)
        {
            try
            {
                await AddImageProcess(addImageDto);
                await _storageProcessor.StorageFileUploadAsync(addImageDto.Content,
                                                            addImageDto.ContentType,
                                                            addImageDto.ImageVariant,
                                                            addImageDto.Name,
                                                            null);
            }
            catch (Exception)
            {
                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);

                throw;
            }
        }

        public async Task RemoveUploadedImageAsync(Guid imageId, string name, ImageVariant imageVariant)
        {
            try
            {
                await RemoveImageDataFromDbAsync(imageId, imageVariant);
                await RemoveImageAsync(name, imageVariant);
            }
            catch (Exception)
            {
                

                throw;
            }
        }

        public async Task RemoveUploadedImageAsync(Guid imageId)
        {
            try
            {
                Image image = await _imageRepository.GetByIdAsnc(imageId);

                if (image == null)
                {
                    throw new NullReferenceException();
                }

                await RemoveMainImageAsync(imageId);
            }
            catch (Exception)
            {


                throw;
            }
        }        

        public async Task RemoveImageThumbnailsAsync(Guid imageId)
        {
            try
            {
                List<ImageStorage> images = _imageStorageRepository.GetByImageId(imageId);

                foreach (ImageStorage imageStorage in images)
                {
                    await RemoveImageAsync(imageStorage.BlobName, imageStorage.imageVariantId);
                    await RemoveThumbnailImageDataFromDbAsync(imageStorage.imageId, imageStorage.imageVariantId);
                }                
            }
            catch (Exception)
            {


                throw;
            }
        }

        public async Task RemoveServiceImageAsync(Guid imageId)
        {
            try
            {
                Image image = await _imageRepository.GetByIdAsnc(imageId);

                if (image == null)
                {
                    throw new NullReferenceException($"There is no image with {imageId} id.");
                }

                var imageVariant = ImageVariant.Service;

                await RemoveImageAsync(imageId, imageVariant);
            }
            catch (Exception)
            {


                throw;
            }
        }

        public async Task RemoveImagesByEventKeyAsync(int eventKey)
        {
            try
            {
                List<Image> images = _imageRepository.GetByEventKey(eventKey);

                if (images == null)
                {
                    throw new NullReferenceException();
                }

                await RemoveImagesAsync(images);
            }
            catch (Exception)
            {


                throw;
            }
        }

        public async Task RemoveImagesByStudioKeyAsync(int studioKey)
        {
            try
            {
                List<Image> images = _imageRepository.GetByStudioKey(studioKey);

                if (images == null)
                {
                    throw new NullReferenceException();
                }

                await RemoveImagesAsync(images);
            }
            catch (Exception)
            {


                throw;
            }
        }

        public async Task UpdateProcessedImagesAsync(AddImageDto addImageDto, AddImageInfoResponseModel hhihAddImageResponse)
        {
            try
            {
                var image = await _imageRepository.GetByIdAsnc(addImageDto.ImageId);
                var imageStorage = _imageStorageRepository.GetByImageIdAndImageVariant(addImageDto.ImageId, addImageDto.ImageVariant);
                var imageApplicationRetention = await _imageApplicationRetentionRepository.GetByIdAsnc(addImageDto.ImageId);

                image.WatermarkMethod = hhihAddImageResponse.WatermarkMethod;
                image.WatermarkImageId = hhihAddImageResponse.WatermarkImageId;
                image.hhihEventKey = hhihAddImageResponse.HHIHEventKey;
                image.hhihPhotographerKey = hhihAddImageResponse.HHIHPhotographerKey;

                imageApplicationRetention.expirationDate = hhihAddImageResponse.ExpirationDate;

                imageStorage.Status = ImageStatus.Ready;

                await _imageRepository.UpdateAsync(image);
                await _imageStorageRepository.UpdateAsync(imageStorage);
                await _imageApplicationRetentionRepository.UpdateAsync(imageApplicationRetention);
            }
            catch (Exception)
            {
                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);
                await RemoveImageAsync(addImageDto.Name, addImageDto.ImageVariant);

                throw;
            }
        }

        public async Task UploadWatermarkImageProcess(AddImageDto addImageDto)
        {
            try
            {
                await AddImageProcess(addImageDto);
                await _storageProcessor.StorageFileUploadAsync(addImageDto.Content,
                                                            addImageDto.ContentType,
                                                            addImageDto.ImageVariant,
                                                            addImageDto.Name,
                                                            null);
            }
            catch (Exception)
            {
                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);

                throw;
            }
        }

        public async Task ThumbnailImagesProcess(GenerateThumbnailImagesDto generateThumbnailImagesDto)
        {
            WaterMarkType watermarkType = WaterMarkType.No;

            byte[] sourceArray = await _storageProcessor
                .StorageFileBytesGetAsync(generateThumbnailImagesDto.FileName, ImageVariant.Main);
            byte[] watermarkArray = null;

            if (generateThumbnailImagesDto.AutoThumbnails && generateThumbnailImagesDto.WatermarkImageId != null)
            {
                var imageVariant = ImageVariant.Service;
                var watermarkImageId = new Guid(generateThumbnailImagesDto.WatermarkImageId);
                var watermarkImageStore = _imageStorageRepository
                        .GetByImageIdAndImageVariant(watermarkImageId, imageVariant);

                if (watermarkImageStore != null)
                {
                    try
                    {
                        watermarkArray = await _storageProcessor
                    .StorageFileBytesGetAsync(watermarkImageStore.BlobName, imageVariant);
                        watermarkType = WaterMarkTypeHelper.GetTypeFromString(generateThumbnailImagesDto.WatermarkMethod);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("ThumbnailImagesProcess Error. Could not get watermark image", ex);
                        throw;
                    } 
                }
            }

            List<ImageStorage> uploadedImages = _imageStorageRepository
                .GetByImageId(generateThumbnailImagesDto.ImageId);

            List<ImageStorageSize> imageSizes = _imageStorageSizeRepository.GetThumbSizes();

            foreach (var size in imageSizes)
            {
                ImageVariant imageVariant = size.imageVariantId;

                bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

                if (isWithWatermarkType && !generateThumbnailImagesDto.AutoThumbnails && watermarkArray == null)
                {
                    _logger.LogInformation($"GenerateThumbnailImages: Skip watermark {imageVariant}. The AutoThumbnails is {generateThumbnailImagesDto.AutoThumbnails} for {imageVariant} image Variant");

                    continue;
                }

                var uploadedImage = uploadedImages.FirstOrDefault(x => x.imageVariantId == imageVariant);

                if (uploadedImage != null && uploadedImage.Status == ImageStatus.Ready)
                {
                    _logger.LogInformation($"GenerateThumbnailImages: The {imageVariant} image Variant already exists");

                    continue;
                }

                string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);
                string fileName = FileHelper.GetFileName(generateThumbnailImagesDto.ImageId.ToString(), filePrefix, generateThumbnailImagesDto.OriginalFileName);

                ImageStorage imageStorageEntity = uploadedImage;

                if (imageStorageEntity == null)
                {
                    imageStorageEntity = ImageStorageExtension.CreateImageStorageEntity(
                        generateThumbnailImagesDto.ImageId,
                        imageVariant,
                        _storageHelper.GetStorageAccountName(imageVariant),
                        _storageHelper.GetContainerName(imageVariant),
                        fileName
                        );

                    await _imageStorageRepository.AddAsync(imageStorageEntity);
                }                              

                using (var resizedStream = isWithWatermarkType ?
                    _imageResizer.ResizeWithWatermark(sourceArray, watermarkArray, size.LongestPixelSize, watermarkType)
                    : _imageResizer.Resize(sourceArray, size.LongestPixelSize))
                {
                    try
                    {
                        await _storageProcessor.StorageFileUploadAsync(resizedStream,
                            generateThumbnailImagesDto.ContentType,
                            imageVariant,
                            fileName,
                            null);

                        imageStorageEntity.Status = ImageStatus.Ready;

                        await _imageStorageRepository.UpdateAsync(imageStorageEntity);
                    }
                    catch (Exception)
                    {
                        _logger.LogError($"GenerateThumbnailImages: StorageFileUploadAsync {imageVariant} Failed. Image data was removed from DB");

                        await _imageStorageRepository.RemoveAsync(imageStorageEntity.imageId, imageVariant);

                        throw;
                    }
                }
            }
        }

        public string GetSaSUrl(AddImageDto addImageDto)
        {
            var expireDateTime = DateTime.Now.AddDays(_blobStorageSettings.SasUrlExpireDateTimeDays);
            var uploadFileAccessUrl = _storageProcessor.UploadFileGetCreateAccessUrl(addImageDto.Name, expireDateTime);

            return uploadFileAccessUrl;
        }

        public async Task<string> GetAccesUrl(Guid imageIdGuid, ImageVariant imageVariant)
        {
            ImageStorageAccessUrl imageStorageAccesUrl = _imageStorageAccesUrlRepository
                                    .GetByImageIdAndImageVariant(imageIdGuid, imageVariant);

            bool wasCreatedNewItem = false;

            if (imageStorageAccesUrl == null)
            {
                imageStorageAccesUrl = new ImageStorageAccessUrl()
                {
                    id = Guid.NewGuid(),
                    imageId = imageIdGuid,
                    imageVariantId = imageVariant,
                    SasUrlExpireDatetime = DateTime.Today
                };

                wasCreatedNewItem = true;
            }

            DateTime sasUrlExpireDatetimeDelta = DateTime.Now
                .AddMinutes(_blobStorageSettings.SasUrlExpireDatetimeDeltaMinutes);

            if (imageStorageAccesUrl.SasUrlExpireDatetime < sasUrlExpireDatetimeDelta)
            {
                ImageStorage imageStorage = _imageStorageRepository
                    .GetByImageIdAndImageVariant(imageIdGuid, imageVariant);

                if (imageStorage == null || imageStorage.Status != ImageStatus.Ready)
                {
                    return string.Empty;
                }
                else
                {
                    DateTimeOffset expireDateTimeOffset = DateTimeOffset.Now
                        .AddMinutes(_blobStorageSettings.StorageContainerUrlExpireMinutes);

                    string sasUrl = await _storageProcessor
                        .StorageFileGetReadAccessUrl(imageVariant, imageStorage.BlobName, expireDateTimeOffset);

                    imageStorageAccesUrl.SaSUrl = sasUrl;
                    imageStorageAccesUrl.SasUrlExpireDatetime = expireDateTimeOffset.DateTime;

                    if (wasCreatedNewItem)
                    {
                        await _imageStorageAccesUrlRepository.AddAsync(imageStorageAccesUrl);
                    }
                    else
                    {
                        await _imageStorageAccesUrlRepository.UpdateAsync(imageStorageAccesUrl);
                    }
                }
            }

            return imageStorageAccesUrl.SaSUrl;
        }

        public async Task<Image> GetImageAsync(Guid imageId)
        {
            return await _imageRepository.GetByIdAsnc(imageId);
        }

        public async Task<ImageUpload> GetImageUpdateAsync(Guid imageId)
        {
            return await _imageUploadRepository.GetByIdAsnc(imageId);
        }

        private async Task AddImageProcess(AddImageDto addImageDto)
        {
            string storageAccauntName = _storageHelper.GetStorageAccountName(addImageDto.ImageVariant);
            string containerName = _storageHelper.GetContainerName(addImageDto.ImageVariant);

            Image imageEntity = addImageDto.CreateImageEntity();
            ImageStorage imageStorage = addImageDto.CreateImageStorageEntity(storageAccauntName, containerName);
            ImageApplicationRetention imageApplicationRetention = addImageDto.CreateImageApplicationRetentionEntity();

            try
            {
                await _imageRepository.AddAsync(imageEntity);
                await _imageStorageRepository.AddAsync(imageStorage);
                await _imageApplicationRetentionRepository.AddAsync(imageApplicationRetention);
            }
            catch (Exception ex)
            {
                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);

                throw;
            }
        }

        private async Task RemoveImageDataFromDbAsync(Guid imageId, ImageVariant imageVariant)
        {
            await _imageRepository.RemoveAsync(imageId);
            await _imageStorageRepository.RemoveAsync(imageId, imageVariant);
            await _imageStorageAccesUrlRepository.RemoveAsync(imageId, imageVariant);
            await _imageApplicationRetentionRepository.RemoveAsync(imageId);
        }

        private async Task RemoveThumbnailImageDataFromDbAsync(Guid imageId, ImageVariant imageVariant)
        {
            
            await _imageStorageRepository.RemoveAsync(imageId, imageVariant);
            await _imageStorageAccesUrlRepository.RemoveAsync(imageId, imageVariant);
        }

        private async Task RemoveImageAsync(string fileName, ImageVariant imageVariant)
        {
            await _storageProcessor.StorageFileDeleteAsync(fileName, imageVariant);
        }

        private async Task RemoveMainImageAsync(Guid imageId)
        {
            var imageVariant = ImageVariant.Main;

            await RemoveImageAsync(imageId, imageVariant);
        }

        private async Task RemoveImageAsync(Guid imageId, ImageVariant imageVariant)
        {
            var imageStorage = _imageStorageRepository.GetByImageIdAndImageVariant(imageId, imageVariant);

            await RemoveImageAsync(imageStorage.BlobName, imageVariant);
            await RemoveImageDataFromDbAsync(imageId, imageVariant);
        }

        private async Task RemoveImagesAsync(List<Image> images)
        {
            if (images == null || images.Count == 0)
            {
                throw new NullReferenceException();
            }

            foreach (Image image in images)
            {
                await RemoveMainImageAsync(image.id);
                await RemoveImageThumbnailsAsync(image.id);
            }
        }
    }
}
