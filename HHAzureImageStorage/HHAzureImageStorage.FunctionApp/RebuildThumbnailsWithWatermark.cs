using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class RebuildThumbnailsWithWatermark
    {
        private readonly ILogger _logger;
        private readonly IImageService _uploadImageService;
        private readonly IHttpHelper _httpHelper;

        public RebuildThumbnailsWithWatermark(ILoggerFactory loggerFactory,
            IHttpHelper httpHelper,
            IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<RebuildThumbnailsWithWatermark>();
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
        }

        [Function("RebuildThumbnailsWithWatermark")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("RebuildThumbnailsWithWatermark: Started");

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            string imageIdValue = formData.GetParameterValue("ImageId")?.Trim();
            string studioKeyValue = formData.GetParameterValue("StudioKey")?.Trim();
            string watermarkMethod = formData.GetParameterValue("WatermarkMethod")?.Trim();

            BaseResponseModel responseModel;

            if (string.IsNullOrEmpty(imageIdValue))
            {
                responseModel = new BaseResponseModel("ImageId is required.", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            var imageId = new Guid(imageIdValue);

            if (imageId == Guid.Empty)
            {
                responseModel = new BaseResponseModel("Input valide ImageId.", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            if (string.IsNullOrEmpty(studioKeyValue))
            {
                responseModel = new BaseResponseModel("ImageId is required.", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            if (!int.TryParse(studioKeyValue, out int studioKey))
            {
                responseModel = new BaseResponseModel("Input valide StudioKey!", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            if (string.IsNullOrEmpty(watermarkMethod))
            {
                responseModel = new BaseResponseModel("WatermarkMethod is required.", false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            RebuildThumbnailsWithWatermarkDto modelDto = new RebuildThumbnailsWithWatermarkDto
            {
                ImageIdStringValue = imageIdValue,
                ImageId = imageId,
                WatermarkMethod = watermarkMethod,
                StudioKey = studioKey
            };

            try
            {
                _logger.LogInformation("RebuildThumbnailsWithWatermark: Started RebuildThumbnailsWithWatermarkAsync");

                await _uploadImageService.RebuildThumbnailsWithWatermarkAsync(modelDto);

                _logger.LogInformation("RebuildThumbnailsWithWatermark: Finished RebuildThumbnailsWithWatermarkAsync");
            }
            catch (Exception ex)
            {
                _logger.LogError($"RebuildThumbnailsWithWatermark: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, $"RebuildThumbnails: Failed.");
            }

            _logger.LogInformation("RebuildThumbnailsWithWatermark: Finished");

            responseModel = new BaseResponseModel($"The images with watermarkId {imageId} was queueted to update");

            return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
        }
    }
}
