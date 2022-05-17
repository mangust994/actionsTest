using Azure.Storage.Queues;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HHAzureImageStorage.IntegrationHHIH;
using HHAzureImageStorage.IntegrationHHIH.Models;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class AddImage
    {
        private readonly IUploadFileHelper _uploadFileHelper;
        private readonly IHttpHelper _httpHelper;

        private readonly HHIHHttpClient _hhihHttpClient;
        private readonly QueueClient _storageQueueClient;
        private readonly IUploadImageService _uploadImageService;

        public AddImage(IUploadFileHelper uploadFileHelper,
            IHttpHelper httpHelper,
            HHIHHttpClient hhihHttpClien,
            QueueClient storageQueueClient,
            IUploadImageService uploadImageService)
        {
            _uploadFileHelper = uploadFileHelper;            
            _hhihHttpClient = hhihHttpClien;
            _storageQueueClient = storageQueueClient;
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
        }

        [Function("AddImage")]
        [OpenApiOperation(operationId: "AddImage", tags: new[] { "image" })]

        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]

        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", bodyType: typeof(BaseResponseModel), Description = "image upload response object")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", bodyType: typeof(string), Description = "required parameters not provided")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/json", bodyType: typeof(string), Description = "forbidden")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestData req,
            FunctionContext context)        
        {
            return new OkObjectResult("VERY NICO");
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

            var queueMessage = System.Text.Json.JsonSerializer.Serialize(generateThumbnailImagesDto);
            await _storageQueueClient.SendMessageAsync(queueMessage);
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
                OriginalFileName = formData.GetParameterValue("OriginalFileName")?.Trim(),
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
