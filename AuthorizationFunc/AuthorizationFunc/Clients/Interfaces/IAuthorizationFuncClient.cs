using AuthorizationFunc.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationFunc.Clients.Interfaces
{
    public interface IAuthorizationFuncClient
    {
        Task<HttpStatusCode> MakeAuthorizationRequest(AuthorizationData authorizationData, ILogger log);
    }
}
