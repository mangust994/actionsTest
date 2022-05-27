using HHAzureImageStorage.BL.Services;
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
    public class DeleteServiceImage
    {
        private readonly ILogger _logger;
        private readonly IHttpHelper _httpHelper;
        private readonly IImageService _uploadImageService;

        public DeleteServiceImage(ILoggerFactory loggerFactory,
            IHttpHelper httpHelper,
            IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<DeleteServiceImage>();            
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
        }

        [Function("DeleteServiceImage")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("DeleteServiceImage: Started");

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
                await _uploadImageService.RemoveServiceImageAsync(imageId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteServiceImage: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, $"DeleteServiceImage: Failed.");
            }

            _logger.LogInformation("DeleteServiceImage: Finished");

            responseModel = new BaseResponseModel($"DeleteServiceImage: The image with id {imageId} was deleted");

            return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
        }
    }
}
