using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AuthorizationFunc.Models;
using AuthorizationFunc.Clients;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using AuthorizationFunc.Parsers;
using System.Security.Claims;
using Microsoft.Azure.WebJobs.Extensions.Http;
using AuthorizationFunc.Clients.Interfaces;
using AuthorizationFunc.Parsers.Interfaces;
using Microsoft.Extensions.Options;
using AuthorizationFunc.Configs;
using System.Text;

namespace AuthorizationFunc
{
    public class AuthorizationFunc
    {
        public IAuthorizationFuncClient authorizationFuncClient { get; set; }

        public IRedisClient redisClient { get; set; }

        public IFormDataParser formDataParser { get; set; }

        public TokenSettings tokenSettings { get; set; }

        public AuthorizationFunc(IAuthorizationFuncClient authorizationFuncClient, 
            IRedisClient redisClient, 
            IFormDataParser formDataParser,
            IOptions<TokenSettings> tokenSettings)
        {
            this.authorizationFuncClient = authorizationFuncClient;
            this.redisClient = redisClient;
            this.formDataParser = formDataParser;
            this.tokenSettings = tokenSettings.Value;
        }

        [FunctionName("AuthorizationFunc")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult("NICO!");
        }
    }
}
