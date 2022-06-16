using HHAzureImageStorage.BL.Extensions;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.BlobStorageProcessor.Interfaces;
using HHAzureImageStorage.BlobStorageProcessor.Settings;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.Core.Models.Responses;
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
    public class ImageService : IImageService
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
        private readonly IQueueMessageService _queueMessageService;

        public ImageService(ILoggerFactory loggerFactory,
            BlobStorageSettings blobStorageOptions,
            IImageRepository imageRepository,
                                IImageStorageRepository imageStorageRepository,
                                IImageApplicationRetentionRepository imageApplicationRetentionRepository,
                                IStorageHelper storageHelper,
                                IImageResizer imageResizer,
                                IImageStorageSizeRepositoty imageStorageSizeRepository,
                                IImageStorageAccessUrlRepository imageStorageAccesUrlRepository,
                                IImageUploadRepository imageUploadRepository,
                                IStorageProcessor storageProcessor,
                                IQueueMessageService queueMessageService)
        {
            _logger = loggerFactory.CreateLogger<ImageService>();
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
            _queueMessageService = queueMessageService;
        }

        public async Task UploadImageProcessAsync(AddImageDto addImageDto)
        {
            _logger.LogInformation($"ImageService| Started UploadImageProcessAsync for {addImageDto.ImageId} ImageId");

            try
            {
                await AddImageProcess(addImageDto);
                await _storageProcessor.StorageFileUploadAsync(addImageDto.Content, addImageDto.ContentType,
                                       addImageDto.ImageVariant, addImageDto.Name, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|UploadImageProcessAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);

                throw;
            }

            _logger.LogInformation($"ImageService| Finished UploadImageProcessAsync for {addImageDto.ImageId} ImageId");
        }

        public async Task RemoveUploadedImageAsync(Guid imageId, string name, ImageVariant imageVariant)
        {
            _logger.LogInformation($"ImageService| Started RemoveUploadedImageAsync for {name} image");

            try
            {
                await RemoveImageDataFromDbAsync(imageId, imageVariant);
                await RemoveImageAsync(name, imageVariant);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|RemoveUploadedImageAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation($"ImageService| Finished RemoveUploadedImageAsync for {name} image");
        }

        public async Task RemoveImageThumbnailsAsync(Guid imageId)
        {
            _logger.LogInformation($"ImageService| Started RemoveImageThumbnailsAsync for {imageId} imageId");

            try
            {
                List<ImageStorage> images = _imageStorageRepository.GetByImageId(imageId);

                foreach (ImageStorage imageStorage in images)
                {
                    await RemoveImageAsync(imageStorage.BlobName, imageStorage.imageVariantId);
                    await RemoveThumbnailImageDataFromDbAsync(imageStorage.imageId, imageStorage.imageVariantId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|RemoveImageThumbnailsAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation($"ImageService| Finished RemoveImageThumbnailsAsync for {imageId} imageId");
        }

        public async Task RemoveServiceImageAsync(Guid imageId)
        {
            _logger.LogInformation($"ImageService| Started RemoveServiceImageAsync for {imageId} imageId");

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
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|RemoveServiceImageAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation($"ImageService| Finished RemoveServiceImageAsync for {imageId} imageId");
        }

        public async Task RemoveImagesByEventKeyAsync(int eventKey)
        {
            _logger.LogInformation($"ImageService| Started RemoveImagesByEventKeyAsync for {eventKey} eventKey");

            try
            {
                List<Image> images = _imageRepository.GetByEventKey(eventKey);

                if (images == null || images.Count == 0)
                {
                    _logger.LogInformation($"ImageService|RemoveImagesByEventKeyAsync: There are no images for event with {eventKey} eventKey");
                }

                await RemoveImagesAsync(images);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|RemoveImagesByEventKeyAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation($"ImageService| Finished RemoveImagesByEventKeyAsync for {eventKey} eventKey");
        }

        public async Task RemoveImagesByStudioKeyAsync(int studioKey)
        {
            _logger.LogInformation($"ImageService| Started RemoveImagesByStudioKeyAsync for {studioKey} studioKey");

            try
            {
                List<Image> images = _imageRepository.GetByStudioKey(studioKey);

                if (images == null || images.Count == 0)
                {
                    _logger.LogInformation($"ImageService|RemoveImagesByStudioKeyAsync: There are no images for studio with {studioKey} studioKey");
                }

                await RemoveImagesAsync(images);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|RemoveImagesByStudioKeyAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }

            _logger.LogInformation($"ImageService| Finished RemoveImagesByStudioKeyAsync for {studioKey} studioKey");
        }

        public async Task UpdateProcessedImagesAsync(AddImageDto addImageDto, AddImageInfoResponseModel hhihAddImageResponse)
        {
            _logger.LogInformation($"ImageService| Started UpdateProcessedImagesAsync for {addImageDto.ImageId} imageId");

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
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|UpdateProcessedImagesAsync: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);
                await RemoveImageAsync(addImageDto.Name, addImageDto.ImageVariant);

                throw;
            }

            _logger.LogInformation($"ImageService| Finished UpdateProcessedImagesAsync for {addImageDto.ImageId} imageId");
        }

        public async Task UploadWatermarkImageProcess(AddImageDto addImageDto)
        {
            _logger.LogInformation($"ImageService| Started UploadWatermarkImageProcess for {addImageDto.ImageId} imageId");

            try
            {
                await AddImageProcess(addImageDto);
                await _storageProcessor.StorageFileUploadAsync(addImageDto.Content,
                                                            addImageDto.ContentType,
                                                            addImageDto.ImageVariant,
                                                            addImageDto.Name,
                                                            null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|UploadWatermarkImageProcess: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);

                throw;
            }

            _logger.LogInformation($"ImageService| Finished UploadWatermarkImageProcess for {addImageDto.ImageId} imageId");
        }

        public async Task ThumbnailImagesProcess(GenerateThumbnailImagesDto generateThumbnailImagesDto)
        {
            _logger.LogInformation($"ImageService| Started ThumbnailImagesProcess for {generateThumbnailImagesDto.ImageId} imageId");

            WaterMarkType watermarkType = WaterMarkType.No;

            byte[] sourceArray = await _storageProcessor
                .StorageFileBytesGetAsync(generateThumbnailImagesDto.FileName, ImageVariant.Main);
            byte[] watermarkArray = null;

            if (generateThumbnailImagesDto.AutoThumbnails && !string.IsNullOrEmpty(generateThumbnailImagesDto.WatermarkImageId))
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

                if (isWithWatermarkType && (!generateThumbnailImagesDto.AutoThumbnails 
                    || watermarkArray == null || watermarkType == WaterMarkType.No))
                {
                    _logger.LogInformation($"GenerateThumbnailImages: Skip watermark {imageVariant}. The AutoThumbnails is {generateThumbnailImagesDto.AutoThumbnails} and WatermarkMethod {generateThumbnailImagesDto.WatermarkMethod} for {imageVariant} image Variant");

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

                ImageResizeResponse resizeResponse = isWithWatermarkType && watermarkType != WaterMarkType.No ?
                    _imageResizer.ResizeWithWatermark(sourceArray, watermarkArray, size.LongestPixelSize, watermarkType)
                    : _imageResizer.Resize(sourceArray, size.LongestPixelSize);

                try
                {
                    await _storageProcessor.StorageFileUploadAsync(resizeResponse.ResizedStream,
                        generateThumbnailImagesDto.ContentType,
                        imageVariant,
                        fileName,
                        null);

                    imageStorageEntity.Status = ImageStatus.Ready;
                    imageStorageEntity.HeightPixels = resizeResponse.HeightPixels;
                    imageStorageEntity.WidthPixels = resizeResponse.WidthPixels;
                    imageStorageEntity.SizeInBytes = resizeResponse.SizeInBytes;

                    await _imageStorageRepository.UpdateAsync(imageStorageEntity);
                }
                catch (Exception)
                {
                    _logger.LogError($"GenerateThumbnailImages: StorageFileUploadAsync {imageVariant} Failed. Image data was removed from DB");

                    await _imageStorageRepository.RemoveAsync(imageStorageEntity.imageId, imageVariant);

                    throw;
                }

                resizeResponse.ResizedStream.Dispose();
            }

            _logger.LogInformation($"ImageService| Finished ThumbnailImagesProcess for {generateThumbnailImagesDto.ImageId} imageId");
        }

        public async Task RebuildThumbnailsWithWatermarkAsync(RebuildThumbnailsWithWatermarkDto modelDto)
        {
            _logger.LogInformation($"ImageService| Started RebuildThumbnailsWithWatermarkAsync for {modelDto.WatermarkImageId} imageId");

            var images = _imageRepository.GetByStudioKeyAndEventKey(modelDto.StudioKey, modelDto.EventKey);

            _logger.LogInformation($"RebuildThumbnailsWithWatermarkAsync: Where are {images.Count} images to rebuild for {modelDto.StudioKey} StudioKey and {modelDto.EventKey} eventKey");

            foreach (var image in images)
            {
                if (image.WatermarkImageId == modelDto.WatermarkImageId && image.WatermarkMethod == modelDto.WatermarkMethod)
                {
                    _logger.LogInformation($"RebuildThumbnailsWithWatermarkAsync: WatermarkImageId is not changed for imageId {image.id} and {modelDto.EventKey} eventKey");

                    continue;
                }

                _logger.LogInformation($"RebuildThumbnailsWithWatermarkAsync: Queue to change for imageId {image.id} , {modelDto.StudioKey} studioKey and {modelDto.EventKey} eventKey");

                string filePrefix = FileHelper.GetFileNamePrefix(ImageVariant.Main);
                string fileName = FileHelper.GetFileName(image.id.ToString(), filePrefix, image.OriginalImageName);

                GenerateThumbnailImagesDto generateThumbnailImagesDto = new GenerateThumbnailImagesDto
                {
                    ImageId = image.id,
                    ContentType = image.MimeType,
                    FileName = fileName,
                    OriginalFileName = image.OriginalImageName,
                    AutoThumbnails = image.AutoThumbnails,
                    WatermarkMethod = modelDto.WatermarkMethod,
                    WatermarkImageId = modelDto.WatermarkImageId,
                    IsRebuildThumbnails = true
                };

                image.WatermarkImageId = modelDto.WatermarkImageId;
                image.WatermarkMethod = modelDto.WatermarkMethod;

                await _imageRepository.UpdateAsync(image);

                await _queueMessageService.SendMessageProcessThumbnailImagesAsync(generateThumbnailImagesDto);
            }

            _logger.LogInformation($"ImageService| Finished RebuildThumbnailsWithWatermarkAsync for {modelDto.WatermarkImageId} imageId");
        }

        public async Task RebuildWatermarkThumbnailsProcess(GenerateThumbnailImagesDto generateThumbnailImagesDto)
        {
            _logger.LogInformation($"ImageService| Started RebuildWatermarkThumbnailsProcess for {generateThumbnailImagesDto.ImageId} imageId");

            WaterMarkType watermarkType = WaterMarkType.No;

            byte[] sourceArray = await _storageProcessor
                .StorageFileBytesGetAsync(generateThumbnailImagesDto.FileName, ImageVariant.Main);
            byte[] watermarkArray = null;

            if (generateThumbnailImagesDto.AutoThumbnails && !string.IsNullOrEmpty(generateThumbnailImagesDto.WatermarkImageId))
            {
                var imageVariant = ImageVariant.Service;
                var watermarkImageId = new Guid(generateThumbnailImagesDto.WatermarkImageId);

                // TODO Get fileName only
                //string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);
                //string fileName = FileHelper.GetFileName(generateThumbnailImagesDto.WatermarkImageId, filePrefix, generateThumbnailImagesDto.OriginalFileName);

                var watermarkImageStore = _imageStorageRepository
                        .GetByImageIdAndImageVariant(watermarkImageId, imageVariant);

                if (watermarkImageStore != null)
                {
                    try
                    {
                        watermarkArray = await _storageProcessor.StorageFileBytesGetAsync(watermarkImageStore.BlobName, imageVariant);
                        watermarkType = WaterMarkTypeHelper.GetTypeFromString(generateThumbnailImagesDto.WatermarkMethod);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ImageService|RebuildWatermarkThumbnailsProcess: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                        throw;
                    }
                }
            }

            List<ImageStorageSize> imageSizes = _imageStorageSizeRepository.GetWatermarkThumbSizes();

            foreach (var size in imageSizes)
            {
                ImageVariant imageVariant = size.imageVariantId;

                _logger.LogInformation($"ImageService|RebuildWatermarkThumbnailsProcess: Processing the {imageVariant.ToString()} for {generateThumbnailImagesDto.ImageId} imageId");

                ImageStorage imageStorageEntity = _imageStorageRepository
                    .GetByImageIdAndImageVariant(generateThumbnailImagesDto.ImageId, imageVariant);

                if (imageStorageEntity == null)
                {
                    _logger.LogInformation("ImageService|RebuildWatermarkThumbnailsProcess: imageStorageEntity is null");

                    if (DoNotNeedThumbGeneration(watermarkType, sourceArray, watermarkArray))
                    {
                        _logger.LogWarning($"RebuildWatermarkThumbnailsProcess: ImageStorage is null & WaterMarkType is No.");

                        continue;
                    }

                    string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);
                    string fileName = FileHelper.GetFileName(generateThumbnailImagesDto.ImageId.ToString(),
                        filePrefix, generateThumbnailImagesDto.OriginalFileName);

                    imageStorageEntity = ImageStorageExtension.CreateImageStorageEntity(
                        generateThumbnailImagesDto.ImageId,
                        imageVariant,
                        _storageHelper.GetStorageAccountName(imageVariant),
                        _storageHelper.GetContainerName(imageVariant),
                        fileName
                        );

                    await _imageStorageRepository.AddAsync(imageStorageEntity);
                }
                else
                {
                    _logger.LogInformation("ImageService|RebuildWatermarkThumbnailsProcess: imageStorageEntity is not null");

                    if (DoNotNeedThumbGeneration(watermarkType, sourceArray, watermarkArray))
                    {
                        _logger.LogWarning($"RebuildWatermarkThumbnailsProcess: WaterMarkType is No. Remove thumbnail for {size.imageVariantId.ToString()}");

                        await _imageStorageRepository.RemoveAsync(imageStorageEntity.imageId, imageVariant);
                        await _imageStorageAccesUrlRepository.RemoveAsync(imageStorageEntity.imageId, imageVariant);

                        await _storageProcessor.StorageFileDeleteAsync(imageStorageEntity.BlobName, imageVariant);

                        continue;
                    }

                    imageStorageEntity.Status = ImageStatus.NeedsRebuild;

                    await _imageStorageRepository.UpdateAsync(imageStorageEntity);
                }

                _logger.LogInformation($"ImageService|RebuildWatermarkThumbnailsProcess: Started ResizeWithWatermark for {imageVariant.ToString()} and {generateThumbnailImagesDto.ImageId} imageId");

                ImageResizeResponse resizeResponse = _imageResizer
                    .ResizeWithWatermark(sourceArray, watermarkArray, size.LongestPixelSize, watermarkType);
                
                try
                {
                    _logger.LogInformation($"ImageService|RebuildWatermarkThumbnailsProcess: Started Upload to storage of {imageStorageEntity.BlobName}");

                    await _storageProcessor.StorageFileUploadAsync(resizeResponse.ResizedStream,
                        generateThumbnailImagesDto.ContentType,
                        imageVariant,
                        imageStorageEntity.BlobName,
                        null);

                    imageStorageEntity.Status = ImageStatus.Ready;
                    imageStorageEntity.HeightPixels = resizeResponse.HeightPixels;
                    imageStorageEntity.WidthPixels = resizeResponse.WidthPixels;
                    imageStorageEntity.SizeInBytes = resizeResponse.SizeInBytes;

                    var image = await _imageRepository.GetByIdAsnc(generateThumbnailImagesDto.ImageId);
                    image.WatermarkMethod = generateThumbnailImagesDto.WatermarkMethod;
                    image.WatermarkImageId = generateThumbnailImagesDto.WatermarkImageId;

                    _logger.LogInformation($"ImageService|RebuildWatermarkThumbnailsProcess: Started Updating db of {imageStorageEntity.BlobName}");

                    await _imageRepository.UpdateAsync(image);
                    await _imageStorageRepository.UpdateAsync(imageStorageEntity);

                    _logger.LogInformation($"ImageService|RebuildWatermarkThumbnailsProcess: Started removing imageStorageAccesUrl of {imageStorageEntity.BlobName}");

                    await _imageStorageAccesUrlRepository.RemoveAsync(generateThumbnailImagesDto.ImageId, imageVariant);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"RebuildWatermarkThumbnailsProcess: StorageFileUploadAsync {imageVariant} Failed.");
                    _logger.LogError($"ImageService|RebuildWatermarkThumbnailsProcess: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                    // TODO What needs to do?
                    //await _imageStorageRepository.RemoveAsync(imageStorageEntity.imageId, imageVariant);

                    throw;
                }

                resizeResponse.ResizedStream.Dispose();
            }

            _logger.LogInformation($"ImageService| Finished RebuildWatermarkThumbnailsProcess");
        }        

        public string GetCreateAccessUrl(string bloabName)
        {
            var expireDateTime = DateTime.Now.AddDays(_blobStorageSettings.UploadsContainerUrlExpireMinutes);

            return _storageProcessor.UploadFileGetCreateAccessUrl(bloabName, expireDateTime);
        }

        public async Task<string> GetAccesUrl(Guid imageIdGuid, ImageVariant imageVariant, int expireInDays)
        {
            _logger.LogInformation($"ImageService| Starteded GetAccesUrl");

            bool shouldUseDefaultDays = 0 == expireInDays;

            if (!shouldUseDefaultDays)
            {
                _logger.LogInformation($"ImageService|GetAccesUrl: Use specific exp day");

                ImageStorage imageStorage = _imageStorageRepository
                    .GetByImageIdAndImageVariant(imageIdGuid, imageVariant);

                if (imageStorage == null || imageStorage.Status != ImageStatus.Ready)
                {
                    _logger.LogInformation($"ImageService|GetAccesUrl: There is no image for {imageVariant.ToString()}");

                    return string.Empty;
                }

                DateTimeOffset expireDateTimeOffset = DateTimeOffset.Now
                        .AddDays(expireInDays);

                return await ReadAccessUrl(expireDateTimeOffset, imageStorage.BlobName, imageVariant);
            }

            ImageStorageAccessUrl imageStorageAccesUrl = _imageStorageAccesUrlRepository
                                    .GetByImageIdAndImageVariant(imageIdGuid, imageVariant);

            bool wasCreatedNewItem = false;

            if (imageStorageAccesUrl == null)
            {
                _logger.LogInformation($"ImageService|GetAccesUrl: There is no imageStorageAccesUrl in db for {imageVariant.ToString()}");

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
                _logger.LogInformation($"ImageService|GetAccesUrl: SasUrlExpireDatetime < sasUrlExpireDatetimeDelta");

                ImageStorage imageStorage = _imageStorageRepository
                    .GetByImageIdAndImageVariant(imageIdGuid, imageVariant);

                if (imageStorage == null || imageStorage.Status != ImageStatus.Ready)
                {
                    _logger.LogInformation($"ImageService|GetAccesUrl: There is no imageStorage in db for {imageVariant.ToString()}");

                    return string.Empty;

                    //// If it is watermark type try get
                    //bool isWithWatermarkType = ImageVariantHelper.IsWithWatermark(imageVariant);

                    //if (!isWithWatermarkType)
                    //{
                    //    return string.Empty;
                    //}

                    //imageVariant = ImageVariantHelper.GetImageVariantOppositeWatermarkVariant(imageVariant);
                    //imageStorage = _imageStorageRepository.GetByImageIdAndImageVariant(imageIdGuid, imageVariant);

                    //if (imageStorage == null || imageStorage.Status != ImageStatus.Ready)
                    //{
                    //    return string.Empty;
                    //}
                }

                DateTimeOffset expireDateTimeOffset = DateTimeOffset.Now
                        .AddDays(_blobStorageSettings.SasUrlExpireDateTimeDays);

                _logger.LogInformation($"ImageService|GetAccesUrl: ReadAccessUrl Started: expireDateTimeOffset - {expireDateTimeOffset}");

                string sasUrl = await ReadAccessUrl(expireDateTimeOffset, imageStorage.BlobName, imageVariant);

                _logger.LogInformation($"ImageService|GetAccesUrl: ReadAccessUrl Finished. sasUrl - {sasUrl}");

                imageStorageAccesUrl.SaSUrl = sasUrl;
                imageStorageAccesUrl.SasUrlExpireDatetime = expireDateTimeOffset.DateTime;

                if (wasCreatedNewItem)
                {
                    _logger.LogInformation($"ImageService|GetAccesUrl: imageStorageAccesUrlRepository.AddAsync");

                    await _imageStorageAccesUrlRepository.AddAsync(imageStorageAccesUrl);
                }
                else
                {
                    _logger.LogInformation($"ImageService|GetAccesUrl: imageStorageAccesUrlRepository.UpdateAsync");

                    await _imageStorageAccesUrlRepository.UpdateAsync(imageStorageAccesUrl);
                }
            }

            _logger.LogInformation($"ImageService|GetAccesUrl: Finished");

            return imageStorageAccesUrl.SaSUrl;
        }

        public async Task<string> GetImageUploadSasUrlAsync(GetImageUploadSasUrlDto addImageDto)
        {
            _logger.LogInformation($"ImageService|GetImageUploadSasUrlAsync: Started");

            ImageUpload imageUploadEntity = addImageDto.CreateImageUploadEntity();

            await _imageUploadRepository.AddAsync(imageUploadEntity);

            _logger.LogInformation($"ImageService|GetImageUploadSasUrlAsync: Finished");

            return GetCreateAccessUrl(addImageDto.FileName);
        }

        public async Task<string> ReadAccessUrl(DateTimeOffset expireDateTimeOffset, string bloabName, ImageVariant imageVariant)
        {
            return await _storageProcessor
                .StorageFileGetReadAccessUrl(imageVariant, bloabName, expireDateTimeOffset);
        }

        public async Task<Image> GetImageAsync(Guid imageId)
        {
            return await _imageRepository.GetByIdAsnc(imageId);
        }

        public async Task<ImageUpload> GetImageImageUploadAsync(Guid imageId)
        {
            return await _imageUploadRepository.GetByIdAsnc(imageId);
        }

        public async Task SetImageReadyStatus(Guid imageId, ImageVariant imageVariant)
        {
            _logger.LogInformation($"ImageService|SetImageReadyStatus: Started");

            var imageStorage = _imageStorageRepository
                                .GetByImageIdAndImageVariant(imageId, imageVariant);

            imageStorage.Status = ImageStatus.Ready;

            await _imageStorageRepository.UpdateAsync(imageStorage);

            _logger.LogInformation($"ImageService|SetImageReadyStatus: Finished");
        }

        public async Task RemoveImageByIdAsync(Guid imageId)
        {
            _logger.LogInformation($"ImageService|RemoveImageByIdAsync: Started");

            await RemoveMainImageAsync(imageId);
            await RemoveImageThumbnailsAsync(imageId);

            _logger.LogInformation($"ImageService|RemoveImageByIdAsync: Finished");
        }

        private async Task AddImageProcess(AddImageDto addImageDto)
        {
            _logger.LogInformation($"ImageService|AddImageProcess: Started");

            string storageAccauntName = _storageHelper.GetStorageAccountName(addImageDto.ImageVariant);
            string containerName = _storageHelper.GetContainerName(addImageDto.ImageVariant);

            _logger.LogInformation($"ImageService|AddImageProcess: Started CreateEntity");

            Image imageEntity = addImageDto.CreateImageEntity();
            ImageStorage imageStorage = addImageDto.CreateImageStorageEntity(storageAccauntName, containerName);
            ImageApplicationRetention imageApplicationRetention = addImageDto.CreateImageApplicationRetentionEntity();

            _logger.LogInformation($"ImageService|AddImageProcess: Finished CreateEntity");

            try
            {
                _logger.LogInformation($"ImageService|AddImageProcess: Started save to db");

                await _imageRepository.AddAsync(imageEntity);
                await _imageStorageRepository.AddAsync(imageStorage);
                await _imageApplicationRetentionRepository.AddAsync(imageApplicationRetention);

                _logger.LogInformation($"ImageService|AddImageProcess: Finished save to db");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageService|AddImageProcess: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                await RemoveImageDataFromDbAsync(addImageDto.ImageId, addImageDto.ImageVariant);

                throw;
            }

            _logger.LogInformation($"ImageService|AddImageProcess: Finished");
        }

        private static bool DoNotNeedThumbGeneration(WaterMarkType watermarkType, byte[] sourceArray, byte[] watermarkArray)
        {
            return watermarkType == WaterMarkType.No || watermarkArray == null || sourceArray == null;
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
            ImageStorage imageStorage = _imageStorageRepository
                .GetByImageIdAndImageVariant(imageId, imageVariant);

            if (imageStorage == null)
            {
                return;
            }

            await RemoveImageAsync(imageStorage.BlobName, imageVariant);
            await RemoveImageDataFromDbAsync(imageId, imageVariant);
        }

        private async Task RemoveImagesAsync(List<Image> images)
        {
            foreach (Image image in images)
            {
                await RemoveImageByIdAsync(image.id);
            }
        }
    }
}
