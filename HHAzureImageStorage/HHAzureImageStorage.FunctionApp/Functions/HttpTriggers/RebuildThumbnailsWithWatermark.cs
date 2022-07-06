using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Functions.HttpTriggers
{
    public class RebuildThumbnailsWithWatermark
    {
        private readonly ILogger _logger;
        private readonly IQueueMessageService _queueMessageService;
        private readonly IHttpHelper _httpHelper;

        public RebuildThumbnailsWithWatermark(ILoggerFactory loggerFactory, IHttpHelper httpHelper, IQueueMessageService queueMessageService)
        {
            _logger = loggerFactory.CreateLogger<RebuildThumbnailsWithWatermark>();
            _httpHelper = httpHelper;
            _queueMessageService = queueMessageService;
        }

        [Function("RebuildThumbnailsWithWatermark")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("RebuildThumbnailsWithWatermark: Started");

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            string imageIdValue = formData.GetParameterValue("ImageId")?.Trim();
            string studioKeyValue = formData.GetParameterValue("StudioKey")?.Trim();
            string eventKeyValue = formData.GetParameterValue("EventKey")?.Trim();
            string watermarkMethod = formData.GetParameterValue("WatermarkMethod")?.Trim();

            BaseResponseModel responseModel;

            if (string.IsNullOrEmpty(studioKeyValue))
            {
                responseModel = new BaseResponseModel("StudioKey is required.", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            if (!int.TryParse(studioKeyValue, out int studioKey))
            {
                responseModel = new BaseResponseModel("Input valide StudioKey!", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            if (string.IsNullOrEmpty(eventKeyValue))
            {
                responseModel = new BaseResponseModel("EventKey is required.", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            if (!int.TryParse(eventKeyValue, out int eventKey))
            {
                responseModel = new BaseResponseModel("Input valide EventKey!", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            if (string.IsNullOrEmpty(watermarkMethod))
            {
                responseModel = new BaseResponseModel("WatermarkMethod is required.", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }
            

            try
            {
                _logger.LogInformation("RebuildThumbnailsWithWatermark: Started SendMessageRebuildThumbnailsWithWatermarkAsync");

                RebuildThumbnailsWithWatermarkDto modelDto = new RebuildThumbnailsWithWatermarkDto
                {
                    WatermarkImageId = imageIdValue,
                    WatermarkMethod = watermarkMethod,
                    StudioKey = studioKey,
                    EventKey = eventKey
                };

                await _queueMessageService.SendMessageRebuildThumbnailsWithWatermarkAsync(modelDto);

                _logger.LogInformation("RebuildThumbnailsWithWatermark: Finished SendMessageRebuildThumbnailsWithWatermarkAsync");
            }
            catch (Exception ex)
            {
                _logger.LogError($"RebuildThumbnailsWithWatermark: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, $"RebuildThumbnails: Failed.");
            }

            _logger.LogInformation("RebuildThumbnailsWithWatermark: Finished");

            responseModel = new BaseResponseModel($"The images from the studio {studioKey} with the event {eventKey} were queued to update");

            return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
        }
    }
}
