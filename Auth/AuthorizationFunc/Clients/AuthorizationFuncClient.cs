using AuthorizationFunc.Clients.Interfaces;
using AuthorizationFunc.Configs;
using AuthorizationFunc.Models;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationFunc.Clients
{
    public class AuthorizationFuncClient : IAuthorizationFuncClient
    {

        public string URI_HIHH_API { get; set; }

        public AuthorizationFuncClient(IOptions<TokenSettings> tokenSettings)
        {
            this.URI_HIHH_API = tokenSettings.Value.URI_HIHH_API;
        }

        public async Task<HttpStatusCode> MakeAuthorizationRequest(AuthorizationData authorizationData, ILogger log)
        {
            string uri = $"{URI_HIHH_API}";
            HttpClient client = new HttpClient();

            try
            {
                log.LogInformation($"Send request to HIHH");
                HttpResponseMessage response;
                var jsonContent = JsonContent.Create<AuthorizationData>(authorizationData);
                response = await client.PostAsync(uri, jsonContent);
                log.LogInformation($"Response from HIHH API returned with code: {response.StatusCode}");
                return response.StatusCode;
            }
            catch (Exception e)
            {
                log.LogError($"Error:{e.Message}. While send request to HIHH");
                return HttpStatusCode.BadRequest;
            }
        }
    }
}
