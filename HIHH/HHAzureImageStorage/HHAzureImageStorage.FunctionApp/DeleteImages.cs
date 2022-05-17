using Azure.Storage.Queues;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class DeleteImages
    {
        private readonly ILogger _logger;
        private readonly IUploadImageService _uploadImageService;
        private readonly IHttpHelper _httpHelper;

        private readonly QueueClient _storageQueueClient;

        public DeleteImages(ILoggerFactory loggerFactory,
            IHttpHelper httpHelper,
            QueueClient storageQueueClient,
            IUploadImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteImages>();
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
            _storageQueueClient = storageQueueClient;
        }

        [Function("DeleteImages")]
        [OpenApiOperation(operationId: "DeleteImages", tags: new[] { "image" })]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("DeleteImage: Started");

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            string eventKeyValue = formData.GetParameterValue("HHIHEventKey")?.Trim();
            string photographerKeyValue = formData.GetParameterValue("HHIHPhotographerKey")?.Trim();

            BaseResponseModel responseModel;

            try
            {
                if (!string.IsNullOrEmpty(photographerKeyValue) && int.TryParse(photographerKeyValue, out int photographerKey))
                {
                    await _uploadImageService.RemoveImagesByStudioKeyAsync(photographerKey);

                    _logger.LogInformation("DeleteImages: Finished");

                    responseModel = new BaseResponseModel($"DeleteImages: All images for studio with {photographerKey} key were deleted");

                    return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
                }

                if (!string.IsNullOrEmpty(eventKeyValue) && int.TryParse(eventKeyValue, out int eventKey))
                {
                    await _uploadImageService.RemoveImagesByEventKeyAsync(eventKey);

                    _logger.LogInformation("DeleteImages: Finished");

                    responseModel = new BaseResponseModel($"DeleteImages: All images for event with {eventKey} key were deleted");

                    return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"DeleteImages: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            responseModel = new BaseResponseModel($"DeleteImage: Failed.", false);

            return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
        }
    }
}
