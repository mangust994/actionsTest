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
    public class GetSaSUrls
    {
        private readonly IHttpHelper _httpHelper;
        private readonly IUploadImageService _uploadImageService;

        public GetSaSUrls(IHttpHelper httpHelper,
            IUploadImageService uploadImageService)
        {
            _httpHelper = httpHelper;
            _uploadImageService = uploadImageService;
        }

        [Function("GetSaSUrls")]

        [OpenApiOperation(operationId: "GetSaSUrls", tags: new[] { "image" })]

        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]

        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(object), Description = "GetSaSUrlsRequestModel", Required = true)]
        //[OpenApiPropertyAttribute()]


        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(HttpResponseData), Description = " response object")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/json", bodyType: typeof(string), Description = "required parameters not provided")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/json", bodyType: typeof(string), Description = "forbidden")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("GetSaSUrls");
            logger.LogInformation("GetSaSUrls: Started");

            GetSaSUrlsResponseModelBase responseModel = new GetSaSUrlsResponseModelBase();

            try
            {
                GetSaSUrlsRequestModel requestModel = await GetRequestModelData(req);

                foreach (ImageVariant imageVariant in requestModel.ImageVariantIds)
                {
                    string accessUrl = await _uploadImageService.GetAccesUrl(requestModel.ImageIdGuid, imageVariant);

                    responseModel.SaSUrls.Add(imageVariant.ToString(), accessUrl);
                }

                if (requestModel.WantImageInfo)
                {
                    responseModel = await GetImageInfo(responseModel, requestModel);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"GetSaSUrls: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, $"GetSaSUrls: Failed.");
            }

            logger.LogInformation("GetSaSUrls: Finished");

            return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, responseModel);
        }

        private async Task<GetSaSUrlsResponseModelExtended> GetImageInfo(GetSaSUrlsResponseModelBase responseModel, GetSaSUrlsRequestModel requestModel)
        {
            Image image = await _uploadImageService.GetImageAsync(requestModel.ImageIdGuid);

            if (image == null)
            {
                return null;
            }

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

            GetSaSUrlsRequestModel requestModel = new GetSaSUrlsRequestModel
            {
                ImageIdGuid = new Guid(formData.GetParameterValue("ImageId")?.Trim()),
                WantImageInfo = wantImageInfo,
                ImageVariantIds = formData.GetParameterValues("ImageVariantId")
                .Select(x => ImageVariantHelper.GetTypeFromString(x?.Trim())).Distinct().ToList()
            };

            return requestModel;
        }
    }
}