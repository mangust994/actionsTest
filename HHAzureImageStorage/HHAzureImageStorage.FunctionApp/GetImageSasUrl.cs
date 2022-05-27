using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.Domain.Entities;
using HHAzureImageStorage.Domain.Enums;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class GetImageSasUrl
    {
        private readonly ILogger _logger;
        private readonly IHttpHelper _httpHelper;
        private readonly IImageService _uploadImageService;

        public GetImageSasUrl(ILoggerFactory loggerFactory, IHttpHelper httpHelper, IImageService uploadImageService)
        {
            _logger = loggerFactory.CreateLogger<GetImageSasUrl>();
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
        }

        [Function("GetImageSasUrl")]
        [OpenApiOperation(operationId: "GetImageSasUrl", tags: new[] { "image" })]

        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]

        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(object), Description = "GetSaSUrlsRequestModel", Required = true)]
        //[OpenApiPropertyAttribute()]


        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(HttpResponseData), Description = " response object")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", bodyType: typeof(string), Description = "required parameters not provided")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/json", bodyType: typeof(string), Description = "forbidden")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req)
        {
            _logger.LogInformation("GetImageSasUrl: Started");

            GetSaSUrlsResponseModelBase responseModel = new GetSaSUrlsResponseModelBase();

            try
            {
                GetSaSUrlsRequestModel requestModel = await GetRequestModelData(req);

                Image image = await _uploadImageService.GetImageAsync(requestModel.ImageIdGuid);

                if (image == null)
                {
                    _logger.LogInformation($"GetImageSasUrl: Image with {requestModel.ImageIdGuid} id is not found");

                    return await _httpHelper.CreateFailedHttpResponseAsync(req, null);
                }                

                foreach (ImageVariant imageVariant in requestModel.ImageVariantIds)
                {
                    _logger.LogInformation($"GetImageSasUrl: Started GetAccesUrl for {imageVariant.ToString()}");

                    string accessUrl = await _uploadImageService.GetAccesUrl(requestModel.ImageIdGuid, imageVariant, requestModel.ExpireInDays);

                    responseModel.SaSUrls.Add(imageVariant.ToString(), accessUrl);

                    _logger.LogInformation($"GetImageSasUrl: Finished GetAccesUrl for {imageVariant.ToString()}");
                }

                if (requestModel.WantImageInfo)
                {
                    _logger.LogInformation("GetImageSasUrl: WantImageInfo is true. Started GetImageInfo}");

                    responseModel = GetImageInfo(image, responseModel);

                    _logger.LogInformation($"GetImageSasUrl: Finished GetImageInfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetImageSasUrl: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, $"GetImageSasUrl: Failed.");
            }

            _logger.LogInformation("GetImageSasUrl: Finished");

            return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
        }

        private GetSaSUrlsResponseModelExtended GetImageInfo(Image image, GetSaSUrlsResponseModelBase responseModel)
        {
            var responseModelExtended = new GetSaSUrlsResponseModelExtended
            {
                SaSUrls = responseModel.SaSUrls,
                HasTransparentAlphaLayer = image.HasTransparentAlphaLayer,
                ColorCorrectLevel = image.ColorCorrectLevel,
                MimeType = image.MimeType,
                OriginalImageName = image.OriginalImageName
            };

            return responseModelExtended;
        }

        private static async Task<GetSaSUrlsRequestModel> GetRequestModelData(HttpRequestData req)
        {
            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

            bool.TryParse(formData.GetParameterValue("WantImageInfo")?.Trim(), out bool wantImageInfo);
            int.TryParse(formData.GetParameterValue("ExpireInDays")?.Trim(), out int expireInDays);

            GetSaSUrlsRequestModel requestModel = new GetSaSUrlsRequestModel
            {
                ImageIdGuid = new Guid(formData.GetParameterValue("ImageId")?.Trim()),
                WantImageInfo = wantImageInfo,
                ExpireInDays = expireInDays,
                ImageVariantIds = formData.GetParameterValues("ImageVariantId")
                .Select(x => ImageVariantHelper.GetTypeFromString(x?.Trim())).Distinct().ToList()
            };

            return requestModel;
        }
    }
}