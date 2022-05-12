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
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# authorization function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var authData = new AuthorizationData
            {
                SecurityKey = formDataParser.ParseValue("SecurityKey", requestBody),
                EventKey = formDataParser.ParseValue("EventKey", requestBody),
                EventUid = formDataParser.ParseValue("EventUid", requestBody)
            };
            string key = $"{authData.SecurityKey}|{authData.EventKey}|{authData.EventUid}";
            var token = await redisClient.GetCahedToken(key);
            if (token == null)
            {
                var statusCode = await authorizationFuncClient.MakeAuthorizationRequest(authData, log);
                if (statusCode == System.Net.HttpStatusCode.OK)
                {
                    var now = DateTime.UtcNow;
                    var jwt = new JwtSecurityToken(
                            issuer: tokenSettings.Issuer,
                            audience: tokenSettings.Audience,
                            notBefore: now,
                            expires: now.Add(TimeSpan.FromMinutes(tokenSettings.Lifetime)),
                            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSettings.Key)), SecurityAlgorithms.HmacSha256));
                    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                    await redisClient.SetCahedToken(key, encodedJwt);
                    return new OkObjectResult(encodedJwt);
                }
                else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new UnauthorizedResult();
                }
                else
                {
                    return new BadRequestResult();
                }
            }
            else
            {
                return new OkObjectResult(token);
            }
        }
    }
}
