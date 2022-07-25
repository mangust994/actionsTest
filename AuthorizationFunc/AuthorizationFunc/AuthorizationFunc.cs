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
using System.Security.Principal;

namespace AuthorizationFunc
{
    public class AuthorizationFunc
    {
       private readonly IAuthorizationFuncClient authorizationFuncClient;

        private readonly IRedisClient redisClient;

        private readonly IFormDataParser formDataParser;

        private readonly TokenSettings tokenSettings;

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
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSettings.Key));
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var authData = new AuthorizationData
            {
                SecurityKey = formDataParser.ParseValue("SecurityKey", requestBody),
                EventKey = formDataParser.ParseValue("EventKey", requestBody),
                EventUid = formDataParser.ParseValue("EventUid", requestBody)
            };
            string key = $"{authData.SecurityKey}|{authData.EventKey}|{authData.EventUid}";
            log.LogInformation("A request is sent to get a token from the cache");
            var token = await redisClient.GetCahedToken(key);
            if (token == null)
            {
                log.LogInformation("No token found for such parameters");
                return await this.ValidateDataAsync(authData, log, securityKey, key);
            }
            else
            {
                log.LogInformation("Token was found, validation begins");
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters() 
                { 
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey = securityKey
                };
                SecurityToken validatedToken;
                try
                {
                    IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                }
                catch (Exception)
                {
                    log.LogInformation("Validation failed");
                    return await this.ValidateDataAsync(authData, log, securityKey, key);
                }
                log.LogInformation("Validation successful, function exited with code 200");
                return new OkObjectResult(token);
            }
        }

        private async Task<IActionResult> ValidateDataAsync(AuthorizationData authData, ILogger log, SymmetricSecurityKey securityKey, string key)
        {
            var statusCode = await authorizationFuncClient.MakeAuthorizationRequest(authData, log);
            if (statusCode == System.Net.HttpStatusCode.OK)
            {
                log.LogInformation("Received code 200, start of token generation");
                var encodedJwt = this.GenerateToken(securityKey);
                await redisClient.SetCahedToken(key, encodedJwt);
                log.LogInformation("Function exited with code 200");
                return new OkObjectResult(encodedJwt);
            }
            else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                log.LogInformation("Function exited with code 401");
                return new UnauthorizedResult();
            }
            else
            {
                log.LogInformation("Function exited with code 400");
                return new BadRequestResult();
            }
        }


        private string GenerateToken(SymmetricSecurityKey securityKey)
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: tokenSettings.Issuer,
                    audience: tokenSettings.Audience,
                    notBefore: now,
                    expires: now.Add(TimeSpan.FromMinutes(tokenSettings.Lifetime)),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
