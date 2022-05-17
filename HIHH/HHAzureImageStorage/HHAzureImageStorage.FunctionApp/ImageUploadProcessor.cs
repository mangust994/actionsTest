// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Storage.Queues;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.FunctionApp.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class ImageUploadProcessor
    {
        private readonly ILogger _logger;
        private readonly IStorageProcessor _storageProcessor;
        private readonly IHttpHelper _httpHelper;
        private readonly IUploadImageService _uploadImageService;

        private readonly QueueClient _storageQueueClient;

        public ImageUploadProcessor(ILoggerFactory loggerFactory, IStorageProcessor storageProcessor,
            IHttpHelper httpHelper,
            QueueClient storageQueueClient,
            IUploadImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<ImageUploadProcessor>();
            _storageProcessor = storageProcessor;
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
            _storageQueueClient = storageQueueClient;
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

                _logger.LogInformation(String.Format("ImageUploadProcessor: Processing URL {0}", createdEvent.Url));

                var uploadFileUri = new Uri(createdEvent.Url);
                string contentType = createdEvent.ContentType;
                var sourceFileName = _storageProcessor.UploadFileGetName(uploadFileUri);
                
                string imageIdValue = Path.GetFileNameWithoutExtension(sourceFileName);
                Guid imageId = new Guid(imageIdValue);

                ImageUpload imageUpload = await _uploadImageService.GetImageUpdateAsync(imageId);
                
                string originalFileName = imageUpload.OriginalImageName;

                Stream fileStream = await _storageProcessor.StorageFileGetAsync(sourceFileName, ImageVariant.Temp);

                var mainImageVariant = ImageVariant.Main;
                var sourceApp = ImageUploader.DirectPost;

                string filePrefix = FileHelper.GetFileNamePrefix(mainImageVariant);
                string fileName = FileHelper.GetFileName(imageId.ToString(), filePrefix, originalFileName);

                AddImageDto addImageDto = AddImageDto.CreateInstance(imageId,
                    fileStream, contentType, originalFileName, fileName, mainImageVariant, sourceApp);

                await _uploadImageService.UploadImageProcessAsync(addImageDto);                

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

                await SendQueueMessageAsync(generateThumbnailImagesDto);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task SendQueueMessageAsync(GenerateThumbnailImagesDto generateThumbnailImagesDto)
        {
            var queueMessage = System.Text.Json.JsonSerializer.Serialize(generateThumbnailImagesDto);
            await _storageQueueClient.SendMessageAsync(queueMessage);
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
