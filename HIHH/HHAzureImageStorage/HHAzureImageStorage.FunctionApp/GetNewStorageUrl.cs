using System;
using System.Net;
using System.Threading.Tasks;
using HHAzureImageStorage.BL.Models.DTOs;
using HHAzureImageStorage.BlobStorageProcessor.Settings;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.DAL.Interfaces;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.FunctionApp.Helpers;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace HHAzureImageStorage.FunctionApp
{
    public class GetNewStorageUrl
    {
        private readonly IUploadFileHelper _uploadFileHelper;
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly IImageUploadRepository _imageUploadRepository;
        private readonly IHttpHelper _httpHelper;
        private readonly IStorageProcessor _storageProcessor;

        public GetNewStorageUrl(BlobStorageSettings blobStorageOptions,
            IUploadFileHelper uploadFileHelper,
            IImageUploadRepository imageUploadRepository,
            IHttpHelper httpHelper,
            IStorageProcessor storageProcessor)
        {
            _uploadFileHelper = uploadFileHelper;
            _blobStorageSettings = blobStorageOptions;
            _imageUploadRepository = imageUploadRepository;
            _httpHelper = httpHelper;
            _storageProcessor = storageProcessor;
        }

        [Function("GetNewStorageUrl")]
        //[OpenApiOperation(operationId: "GetNewStorageUrl", tags: new[] { "image" })]

        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]

        //[OpenApiParameter(name: "OriginalFileName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The cleints unique file name.")]

        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", bodyType: typeof(string), Description = "image upload response object")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", bodyType: typeof(string), Description = "required parameters not provided")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/json", bodyType: typeof(string), Description = "forbidden")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetNewStorageUrl");
            logger.LogInformation("GetNewStorageUrl: Started");

            string responseMessage = "Hello, This HTTP triggered function executed unsuccessfully.";

            try
            {
                AddImageDto addImageDto = await GetImageDto(req, executionContext);
                string uploadFileAccessUrl = GetSaSUrl(addImageDto);

                ImageUpload imageUploadEntity = addImageDto.CreateImageUploadEntity();

                await _imageUploadRepository.AddAsync(imageUploadEntity);

                var response = await _httpHelper.CreateSuccessfulHttpResponseAsync(req, uploadFileAccessUrl);

                logger.LogInformation("GetNewStorageUrl: Finished");

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"GetNewStorageUrl: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseMessage);
            }
        }

        private string GetSaSUrl(AddImageDto addImageDto)
        {
            var expireDateTime = DateTime.UtcNow.AddMinutes(_blobStorageSettings.UploadsContainerUrlExpireMinutes);
            var uploadFileAccessUrl = _storageProcessor.UploadFileGetCreateAccessUrl(addImageDto.Name, expireDateTime);

            return uploadFileAccessUrl;
        }

        private async Task<AddImageDto> GetImageDto(HttpRequestData req, FunctionContext executionContext)
        {
            MultipartFormDataParser formData = await MultipartFormDataParser.ParseAsync(req.Body);

            var originalFileName = formData.GetParameterValue("OriginalFileName");

            originalFileName = _uploadFileHelper.GetValidPhotoName(
                        _uploadFileHelper.Sanitize(originalFileName)
                        );

            AddImageDto addImageDto = AddImageDto.CreateInstance(_uploadFileHelper.GetValidPhotoName(originalFileName));

            SetImageDataFromFormData(addImageDto, formData);

            return addImageDto;
        }

        private static void SetImageDataFromFormData(AddImageDto addImageDto, MultipartFormDataParser formData)
        {
            addImageDto.AutoThumbnails = bool.Parse(formData.GetParameterValue("AutoThumbnails"));
        }
    }
}
