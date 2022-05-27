using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Helpers
{
    public class HttpHelper : IHttpHelper
    {
        public async Task<HttpResponseData> CreateSuccessfulHttpResponseAsync(HttpRequestData req, object data)
        {
            return await CreateHttpResponse(req, data);
        }        

        public async Task<HttpResponseData> CreateFailedHttpResponseAsync(HttpRequestData req, object data)
        {
            return await CreateHttpResponse(req, data, false);
        }

        private static async Task<HttpResponseData> CreateHttpResponse(HttpRequestData req, object data, bool isSuccess = true)
        {
            var response = req.CreateResponse(isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(data);

            return response;
        }
    }
}
