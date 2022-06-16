// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Azure.Messaging.EventGrid.SystemEvents;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.IntegrationHHIH;
using HHAzureImageStorage.IntegrationHHIH.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Functions.Processors
{
    public class ImageUploadProcessor
    {
        private readonly ILogger _logger;
        private readonly IStorageProcessor _storageProcessor;
        private readonly IImageService _uploadImageService;

        private readonly IQueueMessageService _queueMessageService;
        private readonly HHIHHttpClient _hhihHttpClient;

        public ImageUploadProcessor(ILoggerFactory loggerFactory, IStorageProcessor storageProcessor,
            IQueueMessageService queueMessageService, HHIHHttpClient hhihHttpClien, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<ImageUploadProcessor>();
            _storageProcessor = storageProcessor;
            _uploadImageService = uploadImageService;
            _hhihHttpClient = hhihHttpClien;
            _queueMessageService = queueMessageService;
        }

        [Function("ImageUploadProcessor")]
        public async Task Run([EventGridTrigger] MyEvent input)
        {
            _logger.LogInformation(input.Data.ToString());
            _logger.LogInformation("ImageUploadProcessor: Started");

            try
            {
                //Load the blob data
                var createdEvent = JsonSerializer.Deserialize<StorageBlobCreatedEventData>(input.Data.ToString());

                var uploadFileUri = new Uri(createdEvent.Url);
                string contentType = createdEvent.ContentType;
                var sourceFileName = _storageProcessor.UploadFileGetName(uploadFileUri);
                string imageIdValue = Path.GetFileNameWithoutExtension(sourceFileName);

                _logger.LogInformation($"ImageUploadProcessor: Processing URL {createdEvent.Url}|{sourceFileName}|{imageIdValue}");

                Guid imageId = new Guid(imageIdValue);

                ImageUpload imageUpload = await _uploadImageService.GetImageImageUploadAsync(imageId);

                _logger.LogInformation($"ImageUploadProcessor: Started StorageFileGetAsync for {imageId} imageId");

                Stream fileStream = await _storageProcessor.StorageFileGetAsync(sourceFileName, ImageVariant.Temp);

                _logger.LogInformation($"ImageUploadProcessor: Finished StorageFileGetAsync for {imageId} imageId");

                fileStream.Seek(0L, SeekOrigin.Begin);

                var mainImageVariant = ImageVariant.Main;
                var sourceApp = ImageUploader.UI;

                string filePrefix = FileHelper.GetFileNamePrefix(mainImageVariant);
                string fileName = FileHelper.GetFileName(imageId.ToString(), filePrefix, imageUpload.OriginalImageName);

                AddImageDto addImageDto = AddImageDto.CreateInstance(imageId,
                    fileStream, contentType, imageUpload.OriginalImageName, fileName, mainImageVariant, sourceApp);

                addImageDto.AutoThumbnails = imageUpload.AutoThumbnails;
                addImageDto.WatermarkImageId = imageUpload.WatermarkImageId;
                addImageDto.WatermarkMethod = imageUpload.WatermarkMethod;
                addImageDto.HHIHPhotographerKey = imageUpload.hhihPhotographerKey;
                addImageDto.HHIHEventKey = imageUpload.hhihEventKey;

                fileStream.Seek(0L, SeekOrigin.Begin);

                _logger.LogInformation($"ImageUploadProcessor: Started UploadImageProcessAsync for {imageId} imageId");

                await _uploadImageService.UploadImageProcessAsync(addImageDto);
                await _uploadImageService.SetImageReadyStatus(addImageDto.ImageId, addImageDto.ImageVariant);

                _logger.LogInformation($"ImageUploadProcessor: Finished UploadImageProcessAsync for {imageId} imageId");

                AddCloudImageRequestModel hihRequestModel = GetHHIHRequestModel(imageUpload);

                _logger.LogInformation($"ImageUploadProcessor: Started CloudAddPhotoAsync for {imageId} imageId");

                AddImageInfoResponseModel hhihAddImageResponse = await _hhihHttpClient.CloudAddPhotoAsync(hihRequestModel);

                _logger.LogInformation($"ImageUploadProcessor: Finished CloudAddPhotoAsync for {imageId} imageId");

                if (hhihAddImageResponse.Success)
                {
                    _logger.LogInformation("ImageUploadProcessor: CloudAddPhotoAsync - Success");

                    GenerateThumbnailImagesDto generateThumbnailImagesDto = new GenerateThumbnailImagesDto
                    {
                        ImageId = addImageDto.ImageId,
                        ContentType = addImageDto.ContentType,
                        FileName = addImageDto.Name,
                        OriginalFileName = addImageDto.OriginalImageName,
                        AutoThumbnails = imageUpload.AutoThumbnails,
                        WatermarkMethod = imageUpload.WatermarkMethod,
                        WatermarkImageId = imageUpload.WatermarkImageId
                    };

                    _logger.LogInformation($"ImageUploadProcessor: Started SendQueueMessageAsync for {imageId} imageId");

                    await SendQueueMessageAsync(generateThumbnailImagesDto);

                    _logger.LogInformation($"ImageUploadProcessor: Finished SendQueueMessageAsync for {imageId} imageId");

                    _logger.LogInformation("ImageUploadProcessor: Finished");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImageUploadProcessor: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                throw;
            }
        }

        private static AddCloudImageRequestModel GetHHIHRequestModel(ImageUpload imageUpload)
        {
            AddCloudImageRequestModel hihRequestModel = new AddCloudImageRequestModel();

            hihRequestModel.PhotographerKey = imageUpload.hhihPhotographerKey;
            hihRequestModel.EventKey = imageUpload.hhihEventKey;
            hihRequestModel.AlbumKey = imageUpload.AlbumKey;
            hihRequestModel.OriginalFileName = imageUpload.OriginalImageName;
            hihRequestModel.HasTransparentAlphaLayer = imageUpload.HasTransparentAlphaLayer;
            hihRequestModel.HiResDownload = imageUpload.HiResDownload;
            hihRequestModel.CloudImageId = imageUpload.id.ToString();
            hihRequestModel.AutoThumbnails = imageUpload.AutoThumbnails;
            hihRequestModel.SecurityKey = "ReplaceToSecurityKey";// TODO

            return hihRequestModel;
        }

        private async Task SendQueueMessageAsync(GenerateThumbnailImagesDto generateThumbnailImagesDto)
        {
            await _queueMessageService.SendMessageProcessThumbnailImagesAsync(generateThumbnailImagesDto);
        }
    }

    public class MyEvent
    {
        public string Id { get; set; }

        public string Topic { get; set; }

        public string Subject { get; set; }

        public string EventType { get; set; }

        public DateTime EventTime { get; set; }

        public object Data { get; set; }
    }
}
