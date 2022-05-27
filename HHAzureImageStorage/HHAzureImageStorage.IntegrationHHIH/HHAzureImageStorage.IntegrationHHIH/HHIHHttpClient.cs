using HHAzureImageStorage.IntegrationHHIH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HHAzureImageStorage.IntegrationHHIH
{
    public class HHIHHttpClient
    {
        public HttpClient _client { get; }

        public HHIHHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("hhih_api_baseUrl"));
            httpClient.DefaultRequestHeaders.Add("X-hhih-Token", Environment.GetEnvironmentVariable("hhih_api_token"));

            _client = httpClient;
        }

        public async Task<AddImageInfoResponseModel> AutoPostV3AddPhotoAsync(AddImageToHHIHRequestModel requestModel)
        {
            var reguestPath = $"AutoPostV3/AddPhoto";
            return await AddPhotoAsync(requestModel, reguestPath);
        }

        public async Task<AddImageInfoResponseModel> DirectPostV2AddPhotoAsync(AddImageToHHIHRequestModel requestModel)
        {
            var reguestPath = $"DirectPostV2/AddPhoto";

            return await AddPhotoAsync(requestModel, reguestPath);
        }

        public async Task<AddImageInfoResponseModel> CloudAddPhotoAsync(AddCloudImageRequestModel requestModel)
        {
            var reguestPath = $"Cloud/AddPhoto";

            return await AddPhotoAsync(requestModel, reguestPath);
        }

        public async Task<ValidateAutoPostCodeResponse> AutoPostV3ValidateCodeAsync(ValidateAutoPostRequestModel requestModel)
        {
            var reguestPath = $"AutoPostV3/ValidateCode";

            List<KeyValuePair<string, string>> properties = new();

            properties.Add(new KeyValuePair<string, string>("SecurityKey", requestModel.SecurityKey));
            properties.Add(new KeyValuePair<string, string>("AutoPostCode", requestModel.AutoPostCode));
            properties.Add(new KeyValuePair<string, string>("UserId", requestModel.UserId));

            var content = new FormUrlEncodedContent(properties);
            var result = await _client.PostAsync(reguestPath, content);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<ValidateAutoPostCodeResponse>();
        }

        public async Task<ValidateAutoPostCodeResponse> DirectPostV2ValidateCodeAsync(ValidateAutoPostRequestModel requestModel)
        {
            var reguestPath = $"DirectPostV2/ValidateCode";

            List<KeyValuePair<string, string>> properties = new();

            properties.Add(new KeyValuePair<string, string>("SecurityKey", requestModel.SecurityKey));
            properties.Add(new KeyValuePair<string, string>("AutoPostCode", requestModel.AutoPostCode));
            properties.Add(new KeyValuePair<string, string>("UserId", requestModel.UserId));

            var content = new FormUrlEncodedContent(properties);
            var result = await _client.PostAsync(reguestPath, content);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<ValidateAutoPostCodeResponse>();
        }

        private async Task<AddImageInfoResponseModel> AddPhotoAsync(AddImageToHHIHRequestModel requestModel, string requestPath)
        {
            HttpResponseMessage result = await CallPostRequestAsync(requestModel, requestPath);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<AddImageInfoResponseModel>();
        }

        private async Task<AddImageInfoResponseModel> AddPhotoAsync(AddCloudImageRequestModel requestModel, string requestPath)
        {
            HttpResponseMessage result = await CallPostRequestAsync(requestModel, requestPath);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<AddImageInfoResponseModel>();
        }

        private async Task<HttpResponseMessage> CallPostRequestAsync(Object requestModel, string requestPath)
        {
            PropertyInfo[] modelProperies = requestModel.GetType().GetProperties();

            List<KeyValuePair<string, string>> properties = new();

            foreach (var propertyInfo in modelProperies)
            {
                var value = propertyInfo.GetValue(requestModel, null);

                var isList = value is List<string>;

                if (isList)
                {
                    var listValues = value as List<string>;

                    foreach (var listValue in listValues)
                    {
                        properties.Add(new KeyValuePair<string, string>(propertyInfo.Name, listValue));
                    }

                    continue;
                }

                properties.Add(new KeyValuePair<string, string>(propertyInfo.Name, value?.ToString()));
            }

            var content = new FormUrlEncodedContent(properties);
            var result = await _client.PostAsync(requestPath, content);
            return result;
        }
    }
}
