using HHAzureImageStorage.IntegrationHHIH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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

        private async Task<AddImageInfoResponseModel> AddPhotoAsync(AddImageToHHIHRequestModel requestModel, string reguestPath)
        {
            System.Reflection.PropertyInfo[] modelProperies = requestModel.GetType().GetProperties();

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
            var result = await _client.PostAsync(reguestPath, content);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<AddImageInfoResponseModel>();
        }
    }
}
