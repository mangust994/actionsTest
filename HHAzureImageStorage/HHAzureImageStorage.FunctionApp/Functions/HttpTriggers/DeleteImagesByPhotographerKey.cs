using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Functions.HttpTriggers
{
    public class DeleteImagesByPhotographerKey
    {
        private readonly ILogger _logger;
        private readonly IQueueMessageService _queueMessageService;
        private readonly IHttpHelper _httpHelper;

        public DeleteImagesByPhotographerKey(ILoggerFactory loggerFactory, IHttpHelper httpHelper, IQueueMessageService queueMessageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteImagesByPhotographerKey>();
            _httpHelper = httpHelper;
            _queueMessageService = queueMessageService;
        }

        [Function("DeleteImagesByPhotographerKey")]
        [OpenApiOperation(operationId: "DeleteImagesByPhotographerKey", tags: new[] { "image" })]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("DeleteImagesByPhotographerKey: Started");

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            string photographerKeyValue = formData.GetParameterValue("PhotographerKey")?.Trim();

            BaseResponseModel responseModel;

            if (string.IsNullOrEmpty(photographerKeyValue))
            {
                responseModel = new BaseResponseModel("PhotographerKey is required!", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            try
            {
                if (int.TryParse(photographerKeyValue, out int photographerKey))
                {
                    await _queueMessageService.SendMessageDeleteImagesByPhotographerAsync(photographerKey);

                    _logger.LogInformation("DeleteImagesByPhotographerKey: Finished");

                    responseModel = new BaseResponseModel($"All images for studio with {photographerKey} key were queued for deletion");

                    return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"DeleteImagesByPhotographerKey: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            responseModel = new BaseResponseModel($"Failed.", false);

            return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
        }
    }
}
