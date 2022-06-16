using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Functions.HttpTriggers
{
    public class DeleteImagesByEventKey
    {
        private readonly ILogger _logger;
        private readonly IQueueMessageService _queueMessageService;
        private readonly IHttpHelper _httpHelper;

        public DeleteImagesByEventKey(ILoggerFactory loggerFactory, IHttpHelper httpHelper, IQueueMessageService queueMessageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteImagesByEventKey>();
            _httpHelper = httpHelper;
            _queueMessageService = queueMessageService;
        }

        [Function("DeleteImagesByEventKey")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("DeleteImagesByEventKey: Started");

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            string eventKeyValue = formData.GetParameterValue("EventKey")?.Trim();

            BaseResponseModel responseModel;

            if (string.IsNullOrEmpty(eventKeyValue))
            {
                responseModel = new BaseResponseModel("EventKey is required!", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            try
            {
                if (int.TryParse(eventKeyValue, out int eventKey))
                {
                    await _queueMessageService.SendMessageDeleteImagesByEventAsync(eventKey);

                    _logger.LogInformation("DeleteImagesByEventKey: Finished");

                    responseModel = new BaseResponseModel($"All images for event with {eventKey} key were queued for deletion");

                    return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"DeleteImagesByEventKey: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");
            }

            responseModel = new BaseResponseModel($"Failed.", false);

            return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
        }
    }
}
