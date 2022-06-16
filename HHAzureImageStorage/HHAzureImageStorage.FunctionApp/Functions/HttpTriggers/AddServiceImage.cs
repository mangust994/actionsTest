using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Functions.HttpTriggers
{
    public class AddServiceImage
    {
        private readonly ILogger _logger;

        private readonly IUploadFileHelper _uploadFileHelper;
        private readonly IHttpHelper _httpHelper;

        private readonly IImageService _uploadImageService;

        public AddServiceImage(ILoggerFactory loggerFactory,
            IUploadFileHelper uploadFileHelper,
            IHttpHelper httpHelper,
            IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<AddServiceImage>();
            _uploadFileHelper = uploadFileHelper;
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
        }

        [Function("AddServiceImage")]
        [OpenApiOperation(operationId: "AddServiceImage", tags: new[] { "image" })]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("AddServiceImage: Started");

            string responseMessage = "AddServiceImage function executed unsuccessfully.";

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            string errorMessage = _uploadFileHelper.ValidateFile(formData);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return await _httpHelper.CreateFailedHttpResponseAsync(req, errorMessage);
            }

            using (MemoryStream fileStream = new MemoryStream())
            {
                var file = formData.Files[0];
                await file.Data.CopyToAsync(fileStream);
                fileStream.Seek(0L, SeekOrigin.Begin);

                string originalFileName = _uploadFileHelper.GetValidPhotoName(
                        _uploadFileHelper.Sanitize(file.FileName)
                        );

                var imageId = Guid.NewGuid();
                var imageVariant = ImageVariant.Service;
                var sourceApp = ImageUploader.UI;

                string filePrefix = FileHelper.GetFileNamePrefix(imageVariant);
                string fileName = FileHelper.GetFileName(imageId.ToString(), filePrefix, originalFileName);

                AddImageDto addImageDto = AddImageDto.CreateInstance(imageId,
                    fileStream, file.ContentType, originalFileName, fileName, imageVariant, sourceApp);

                try
                {
                    await _uploadImageService.UploadWatermarkImageProcess(addImageDto);

                    await _uploadImageService.SetImageReadyStatus(addImageDto.ImageId, addImageDto.ImageVariant);

                    var responseModel = new BaseResponseModel(addImageDto.ImageId.ToString());

                    _logger.LogInformation("AddServiceImage: Finished");

                    return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError("AddServiceImage Error.", ex);

                    responseMessage += ex.Message;
                }
            }

            _logger.LogInformation("AddServiceImage: Finished");

            return await _httpHelper.CreateFailedHttpResponseAsync(req, responseMessage);
        }
    }
}
