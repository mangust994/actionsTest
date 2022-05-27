using HHAzureImageStorage.BL.Extensions;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class GetImageUploadSasUrl
    {
        private readonly ILogger _logger;
        private readonly IUploadFileHelper _uploadFileHelper;
        private readonly IHttpHelper _httpHelper;

        private readonly IImageService _uploadImageService;

        public GetImageUploadSasUrl(ILoggerFactory loggerFactory, IUploadFileHelper uploadFileHelper,
            IHttpHelper httpHelper, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<GetImageUploadSasUrl>();
            _uploadFileHelper = uploadFileHelper;
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
        }

        [Function("GetImageUploadSasUrl")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("GetImageUploadSasUrl: Started");

            BaseResponseModel responseModel;

            try
            {
                MultipartFormDataParser formData = await MultipartFormDataParser.ParseAsync(req.Body);

                GetImageUploadSasUrlRequestModel requestModel = SetImageDataFromFormData(formData);

                string originalFileName = _uploadFileHelper
                   .GetValidPhotoName(_uploadFileHelper.Sanitize(requestModel.OriginalFileName.Default()));

                _logger.LogInformation($"AddImage: OriginalFileName is {originalFileName}");

                requestModel.OriginalFileName = originalFileName;

                //errorMessage = ValidateRequestModel(isDirectPost, errorMessage, requestModel, originalFileName);

                //if (!string.IsNullOrEmpty(errorMessage))
                //{
                //    responseModel = new BaseResponseModel(errorMessage, false);

                //    return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
                //}

                GetImageUploadSasUrlDto addImageDto = GetImageUploadSasUrlDto.CreateInstance(_uploadFileHelper.GetValidPhotoName(originalFileName));
                
                SetDataForDto(requestModel, addImageDto);

                _logger.LogInformation("GetImageUploadSasUrl: Started GetImageUploadSasUrlAsync");

                string uploadFileAccessUrl = await _uploadImageService.GetImageUploadSasUrlAsync(addImageDto);

                _logger.LogInformation("GetImageUploadSasUrl: GetImageUploadSasUrlAsync");
                _logger.LogInformation("GetImageUploadSasUrl: Finished");

                responseModel = new BaseResponseModel(uploadFileAccessUrl);

                return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetImageUploadSasUrl: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                responseModel = new BaseResponseModel(ex.Message, false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }
        }

        private static void SetDataForDto(GetImageUploadSasUrlRequestModel requestModel, GetImageUploadSasUrlDto addImageDto)
        {
            addImageDto.PhotographerKey = requestModel.PhotographerKey;
            addImageDto.EventKey = requestModel.EventKey;
            addImageDto.AlbumKey = requestModel.AlbumKey;
            addImageDto.ColorCorrectLevel = requestModel.ColorCorrectLevel;
            addImageDto.HiResDownload = requestModel.HiResDownload;
            addImageDto.AutoThumbnails = requestModel.AutoThumbnails;
            addImageDto.WatermarkImageId = requestModel.WatermarkImageId;
            addImageDto.WatermarkMethod = requestModel.WatermarkMethod;
            addImageDto.HasTransparentAlphaLayer = requestModel.HasTransparentAlphaLayer;
        }

        private static GetImageUploadSasUrlRequestModel SetImageDataFromFormData(MultipartFormDataParser formData)
        {
            int.TryParse(formData.GetParameterValue("PhotographerKey")?.Trim(), out int studioKey);
            int.TryParse(formData.GetParameterValue("EventKey")?.Trim(), out int eventKey);
            int.TryParse(formData.GetParameterValue("AlbumKey")?.Trim(), out int albumKey);
            bool.TryParse(formData.GetParameterValue("ColorCorrectLevel")?.Trim(), out bool isColorCorrected);
            bool.TryParse(formData.GetParameterValue("AutoThumbnails")?.Trim(), out bool autoThumbnails);
            bool.TryParse(formData.GetParameterValue("HiResDownload")?.Trim(), out bool hiResDownload);
            bool.TryParse(formData.GetParameterValue("HasTransparentAlphaLayer")?.Trim(), out bool hasTransparentAlphaLayer);
            DateTime.TryParse(formData.GetParameterValue("ExpirationDate")?.Trim(), out DateTime expirationDate);
            
            GetImageUploadSasUrlRequestModel requestModel = new GetImageUploadSasUrlRequestModel
            {
                PhotographerKey = studioKey,
                EventKey = eventKey,
                AlbumKey = albumKey,
                OriginalFileName = formData.GetParameterValue("OriginalFileName")?.Trim(),
                ColorCorrectLevel = isColorCorrected,
                AutoThumbnails = autoThumbnails,
                HiResDownload = hiResDownload,
                HasTransparentAlphaLayer = hasTransparentAlphaLayer,
                WatermarkImageId = formData.GetParameterValue("WatermarkImageId")?.Trim(),
                WatermarkMethod = formData.GetParameterValue("WatermarkMethod")?.Trim(),
                ExpirationDate = expirationDate
            };

            return requestModel;
        }

        private static string ValidateRequestModel(bool isDirectPost, string errorMessage, AddImageRequestModel requestModel, string originalFileName)
        {
            StringBuilder errorMessageBuilder = new StringBuilder(errorMessage);

            if (string.IsNullOrEmpty(requestModel.SecurityKey))
            {
                errorMessageBuilder.Append(" SecurityKey is required.");
            }

            if (!isDirectPost && string.IsNullOrEmpty(requestModel.AutoPostCode))
            {
                errorMessageBuilder.Append(" AutoPostCode is required.");
            }

            if (string.IsNullOrEmpty(requestModel.GroupName))
            {
                errorMessageBuilder.Append(" Group Name is required.");
            }

            if (!string.IsNullOrEmpty(requestModel.Password) && requestModel.Password.Length > 100)
            {
                errorMessageBuilder.Append(" Password cannot be longer than 100 characters.");
            }

            if (originalFileName.Length > 300)
            {
                errorMessageBuilder.Append(" Photo name cannot be longer than 300 characters.");
            }

            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                errorMessageBuilder.Append(" Photo name is required.");
            }

            return errorMessageBuilder.ToString();
        }
    }
}
