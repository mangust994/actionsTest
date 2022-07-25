using HHAzureImageStorage.BL.Extensions;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HHAzureImageStorage.IntegrationHHIH;
using HHAzureImageStorage.IntegrationHHIH.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Functions.HttpTriggers
{
    public class AddImage
    {
        private readonly ILogger _logger;
        private readonly IUploadFileHelper _uploadFileHelper;
        private readonly IHttpHelper _httpHelper;
        private readonly IImageService _uploadImageService;

        private readonly HHIHHttpClient _hhihHttpClient;
        private readonly IQueueMessageService _queueMessageService;

        public AddImage(ILoggerFactory loggerFactory, IUploadFileHelper uploadFileHelper,
            IHttpHelper httpHelper, HHIHHttpClient hhihHttpClien, IQueueMessageService queueMessageService,
            IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<AddImage>();
            _uploadFileHelper = uploadFileHelper;
            _hhihHttpClient = hhihHttpClien;
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
            _queueMessageService = queueMessageService;
        }

        [Function("AddPhoto")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req)
        {
            _logger.LogInformation("AddImage: Started");

            string responseMessage = string.Empty;
            BaseResponseModel responseModel;

            bool isDirectPost = false;

            if (req.Headers.Contains("SubName"))
            {
                var subName = req.Headers.GetValues("SubName").FirstOrDefault();

                if (!string.IsNullOrEmpty(subName) && subName == "DirectPost")
                {
                    isDirectPost = true;

                    _logger.LogInformation("The header for DirectPost exists");
                }
            }

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

#if DEBUG
            if (bool.TryParse(formData.GetParameterValue("IsDirectPost"), out bool isDirectPostFromParam))
            {
                isDirectPost = isDirectPostFromParam;
            }
#endif

            _logger.LogInformation("AddImage: Started ValidateFile");

            string errorMessage = _uploadFileHelper.ValidateFile(formData);

            _logger.LogInformation("AddImage: Finisheded ValidateFile");

            AddImageRequestModel requestModel = GetAddImageRequestModelFromFormData(formData);

            string originalFileName = _uploadFileHelper
                .GetValidPhotoName(_uploadFileHelper.Sanitize(requestModel.OriginalFileName.Default()));

            _logger.LogInformation($"AddImage: OriginalFileName is {originalFileName}");

            errorMessage = ValidateRequestModel(isDirectPost, errorMessage, requestModel, originalFileName);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                responseModel = new BaseResponseModel(errorMessage, false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            requestModel.OriginalFileName = originalFileName;

            var imageId = Guid.NewGuid();
            var imageVariant = ImageVariant.Main;

            _logger.LogInformation($"AddImage: imageId is {imageId}");

            string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);
            string fileName = FileHelper.GetFileName(imageId.ToString(), filePrefix, originalFileName);

            var sourceApp = isDirectPost ? ImageUploader.DirectPost : ImageUploader.AutoPost;

            _logger.LogInformation($"AddImage: sourceApp is {sourceApp}");

            try
            {
                using (MemoryStream fileStream = new MemoryStream())
                {
                    _logger.LogInformation($"AddImage: Started file.Data.CopyToAsync(fileStream) for {imageId} imageId");

                    var file = formData.Files[0];
                    await file.Data.CopyToAsync(fileStream);
                    fileStream.Seek(0L, SeekOrigin.Begin);
                    string contentType = FileHelper.GetMineType(originalFileName);

                    _logger.LogInformation($"AddImage: Finished file.Data.CopyToAsync(fileStream) for {imageId} imageId");

                    AddImageDto addImageDto = AddImageDto.CreateInstance(imageId,
                        fileStream, contentType, originalFileName, fileName,
                        imageVariant, sourceApp);

                    AddImageDto.SetImageData(imageId, fileStream, contentType,
                        _logger, addImageDto);

                    _logger.LogInformation($"AddImage: SetImageDataFromRequestModel for {imageId} imageId");

                    SetImageDataFromRequestModel(addImageDto, requestModel);

                    _logger.LogInformation($"AddImage: Started UploadImageProcessAsync for {imageId} imageId");

                    await _uploadImageService.UploadImageProcessAsync(addImageDto);

                    _logger.LogInformation($"AddImage: Finished UploadImageProcessAsync for {imageId} imageId");

                    AddImageToHHIHRequestModel hihRequestModel =
                        GetHHIHRequestModelFromRequestModel(requestModel,
                            addImageDto.ImageId.ToString(), addImageDto.HasTransparentAlphaLayer);

                    _logger.LogInformation($"AddImage: Started AddImageToHHIHRequest for {imageId} imageId");

                    AddImageInfoResponseModel hhihAddImageResponse =
                        !isDirectPost ? await _hhihHttpClient.AutoPostV3AddPhotoAsync(hihRequestModel)
                        : await _hhihHttpClient.DirectPostV2AddPhotoAsync(hihRequestModel);

                    _logger.LogInformation($"AddImage: Finished AddImageToHHIHRequest for {imageId} imageId");

                    if (hhihAddImageResponse.Success)
                    {
                        _logger.LogInformation($"AddImage: Started UpdateProcessedImagesAsync for {imageId} imageId");

                        await _uploadImageService.UpdateProcessedImagesAsync(addImageDto.ImageId, addImageDto.ImageVariant, hhihAddImageResponse);

                        _logger.LogInformation($"AddImage: Finished UpdateProcessedImagesAsync for {imageId} imageId");

                        _logger.LogInformation($"AddImage: Started SendQueueMessageAsync for {imageId} imageId");

                        await SendQueueMessageAsync(addImageDto, hhihAddImageResponse);

                        _logger.LogInformation($"AddImage: Finished SendQueueMessageAsync for {imageId} imageId");

                        responseModel = new BaseResponseModel(addImageDto.ImageId.ToString());

                        _logger.LogInformation("AddImage: Finished");

                        return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
                    }

                    _logger.LogInformation($"AddImage: Started RemoveUploadedImageAsync for {imageId} imageId");

                    await _uploadImageService.RemoveUploadedImageAsync(imageId, fileName, imageVariant);

                    _logger.LogInformation($"AddImage: Finished RemoveUploadedImageAsync for {imageId} imageId");

                    var hihErrorMessage = System.Text.Json.JsonSerializer.Serialize(hhihAddImageResponse);

                    _logger.LogInformation($"AddImage: hhihAddImageResponse is - {hhihAddImageResponse} for {imageId} imageId");

                    responseMessage = $"AddImage - hhihAddImageResponse executed unsuccessfully {hihErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddImage: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                _logger.LogInformation($"AddImage: Started RemoveUploadedImageAsync for {imageId} imageId");

                await _uploadImageService.RemoveUploadedImageAsync(imageId, fileName, imageVariant);

                _logger.LogInformation($"AddImage: Finished RemoveUploadedImageAsync for {imageId} imageId");

                responseMessage += ex.Message;
            }

            _logger.LogInformation("AddImage: Finished");

            responseModel = new BaseResponseModel(responseMessage, false);

            return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
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

        private async Task SendQueueMessageAsync(AddImageDto addImageDto, AddImageInfoResponseModel hhihAddImageResponse)
        {
            GenerateThumbnailImagesDto generateThumbnailImagesDto = new GenerateThumbnailImagesDto
            {
                ImageId = addImageDto.ImageId,
                ContentType = addImageDto.ContentType,
                FileName = addImageDto.Name,
                OriginalFileName = addImageDto.OriginalImageName,
                AutoThumbnails = hhihAddImageResponse.AutoThumbnails,
                WatermarkMethod = hhihAddImageResponse.WatermarkMethod,
                WatermarkImageId = hhihAddImageResponse.WatermarkImageId
            };

            await _queueMessageService.SendMessageProcessThumbnailImagesAsync(generateThumbnailImagesDto);
        }

        private static AddImageToHHIHRequestModel GetHHIHRequestModelFromRequestModel(AddImageRequestModel requestModel,
            string imageId, bool hasTransparentAlphaLayer)
        {
            // TODO replace to automapping
            return new AddImageToHHIHRequestModel
            {
                SecurityKey = requestModel.SecurityKey,
                GroupName = requestModel.GroupName,
                Password = requestModel.Password,
                FreeHiResDownload = requestModel.FreeHiResDownload,
                AddIfAlreadyExists = requestModel.AddIfAlreadyExists,
                OverwriteExistingImages = requestModel.OverwriteExistingImages,
                OriginalFileName = requestModel.OriginalFileName,
                EmailAddress = requestModel.EmailAddress,
                CellNumber = requestModel.CellNumber,
                CloudImageId = imageId,
                AdditionalEmailAddresses = requestModel.AdditionalEmailAddresses,
                AdditionalCellNumbers = requestModel.AdditionalCellNumbers,
                AutoPostCode = requestModel.AutoPostCode,
                IsColorCorrected = requestModel.IsColorCorrected,
                EventKey = requestModel.EventKey,
                EventUid = requestModel.EventUid,
                HasTransparentAlphaLayer = hasTransparentAlphaLayer
            };
        }

        private static void SetImageDataFromRequestModel(AddImageDto addImageDto, AddImageRequestModel requestModel)
        {
            addImageDto.ColorCorrectLevel = requestModel.IsColorCorrected;
            addImageDto.AutoThumbnails = true; // TODO
        }

        private static AddImageRequestModel GetAddImageRequestModelFromFormData(MultipartFormDataParser formData)
        {
            bool.TryParse(formData.GetParameterValue("FreeHiResDownload")?.Trim(), out bool freeHiResDownload);
            bool.TryParse(formData.GetParameterValue("AddIfAlreadyExists")?.Trim(), out bool addIfAlreadyExists);
            bool.TryParse(formData.GetParameterValue("OverwriteExistingImages")?.Trim(), out bool overwriteExistingImages);
            bool.TryParse(formData.GetParameterValue("IsColorCorrected")?.Trim(), out bool isColorCorrected);

            AddImageRequestModel requestModel = new AddImageRequestModel
            {
                SecurityKey = formData.GetParameterValue("SecurityKey")?.Trim(),
                GroupName = formData.GetParameterValue("GroupName")?.Trim(),
                Password = formData.GetParameterValue("Password")?.Trim(),
                FreeHiResDownload = freeHiResDownload,
                AddIfAlreadyExists = addIfAlreadyExists,
                OverwriteExistingImages = overwriteExistingImages,
                OriginalFileName = formData.GetParameterValue("OriginalFilename")?.Trim(),
                EmailAddress = formData.GetParameterValue("EmailAddress")?.Trim(),
                CellNumber = formData.GetParameterValue("CellNumber")?.Trim(),
                AutoPostCode = formData.GetParameterValue("AutoPostCode")?.Trim(),
                IsColorCorrected = isColorCorrected,
                EventKey = formData.GetParameterValue("EventKey")?.Trim(),
                EventUid = formData.GetParameterValue("EventUid")?.Trim(),
                AdditionalEmailAddresses = formData.GetParameterValues("AdditionalEmailAddresses").ToList(),
                AdditionalCellNumbers = formData.GetParameterValues("AdditionalCellNumbers").ToList()
            };

            return requestModel;
        }
    }
}
