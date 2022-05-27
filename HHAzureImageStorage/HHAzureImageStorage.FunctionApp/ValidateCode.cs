using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.FunctionApp.Models;
using HHAzureImageStorage.IntegrationHHIH;
using HHAzureImageStorage.IntegrationHHIH.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp
{
    public class ValidateCode
    {
        private readonly ILogger _logger;
        private readonly IHttpHelper _httpHelper;

        private readonly HHIHHttpClient _hhihHttpClient;

        public ValidateCode(ILoggerFactory loggerFactory, IHttpHelper httpHelper, HHIHHttpClient hhihHttpClien)
        {
            _logger = loggerFactory.CreateLogger<ValidateCode>();
            _hhihHttpClient = hhihHttpClien;
            _httpHelper = httpHelper;
        }

        [Function("ValidateCode")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("ValidateCode: Started");

            var formData = await MultipartFormDataParser.ParseAsync(req.Body);

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


#if DEBUG
            if (bool.TryParse(formData.GetParameterValue("IsDirectPost"), out bool isDirectPostFromParam))
            {
                isDirectPost = isDirectPostFromParam;
            }
#endif

            string securityKey = formData.GetParameterValue("SecurityKey")?.Trim();
            string autoPostCode = formData.GetParameterValue("AutoPostCode")?.Trim();
            string userId = formData.GetParameterValue("UserId")?.Trim();

            BaseResponseModel responseModel;
            StringBuilder errorMessage = new StringBuilder();

            if (string.IsNullOrEmpty(securityKey))
            {
                errorMessage.Append("SecurityKey is required.");
            }

            if (string.IsNullOrEmpty(autoPostCode) || autoPostCode.Length < 12)
            {
                errorMessage.Append(" AutoPost Code too short.");
            }

            if (isDirectPost && string.IsNullOrEmpty(userId))
            {
                errorMessage.Append(" UserId is required.");
            }

            string errorMessageText = errorMessage.ToString();

            if (!string.IsNullOrEmpty(errorMessageText))
            {
                responseModel = new BaseResponseModel(errorMessageText, false);

                return await _httpHelper.CreateFailedHttpResponseAsync(req, responseModel);
            }

            try
            {
                ValidateAutoPostRequestModel requestModel = new ValidateAutoPostRequestModel
                {
                    SecurityKey = securityKey,
                    AutoPostCode = autoPostCode,
                    UserId = userId
                };

                ValidateAutoPostCodeResponse hhihResponse = !isDirectPost ? await _hhihHttpClient.DirectPostV2ValidateCodeAsync(requestModel)
                    : await _hhihHttpClient.AutoPostV3ValidateCodeAsync(requestModel);

                _logger.LogInformation("ValidateCode: Finished");

                return await _httpHelper.CreateSuccessfulHttpResponseAsync(req, hhihResponse);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"ValidateCode: Failed. Exception Message: {ex.Message} : Stack: {ex.StackTrace}");

                return await _httpHelper.CreateFailedHttpResponseAsync(req, $"ValidateCode: Failed.");
            }
        }
    }
}
