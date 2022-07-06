using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;

namespace HHAzureImageStorage.FunctionApp.Helpers
{
    public interface IHttpHelper
    {
        public Task<HttpResponseData> CreateSuccessfulHttpResponseAsync(HttpRequestData req, object data);

        public Task<HttpResponseData> CreateFailedHttpResponseAsync(HttpRequestData req, object data);
    }
}
