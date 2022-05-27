using Azure.Storage.Queues;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class DeleteImage
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;
        private readonly IHttpHelper _httpHelper;

        private readonly QueueClient _storageQueueClient;

        public DeleteImage(ILoggerFactory loggerFactory,
            IHttpHelper httpHelper,
            QueueClient storageQueueClient,
            IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteImage>();
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
            _storageQueueClient = storageQueueClient;
        }

        [Function("DeleteImage")]
        [OpenApiOperation(operationId: "DeleteImage", tags: new[] { "image" })]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("DeleteImage: Started");

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            string imageIdValue = formData.GetParameterValue("ImageId")?.Trim();

            BaseResponseModel responseModel;

            if (string.IsNullOrEmpty(imageIdValue))
            {
                responseModel = new BaseResponseModel("ImageId is required!", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            var imageId = new Guid(imageIdValue);

            if (imageId == Guid.Empty)
            {
                responseModel = new BaseResponseModel("Input valide ImageId!", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            try
            {
                // TODO Send message to the queue
                await _uploadImageService.RemoveUploadedImageAsync(imageId);
                await _uploadImageService.RemoveImageThumbnailsAsync(imageId);                
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteImage: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, $"DeleteImage: Failed.");
            }

            _logger.LogInformation("DeleteImage: Finished");

            responseModel = new BaseResponseModel($"DeleteImage: The image with id {imageId} was deleted");

            return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
        }
    }
}
