using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthorizationFunc.Clients.Interfaces;
using AuthorizationFunc.Configs;
using AuthorizationFunc.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AuthorizationFunc.Clients
{
    public class RedisClient : IRedisClient
    {

        private IDatabase db;

        public RedisClient(IOptions<TokenSettings> tokenSettings)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(tokenSettings.Value.RedisConnectionString);
            this.db = redis.GetDatabase();
        }

        public async Task<string> GetCahedToken(string key)
        {
            string token = await db.StringGetAsync(key);
            return token;
        }

        public async Task SetCahedToken(string key, string token)
        {
            string serializedToken = token;
            await db.StringSetAsync(key, serializedToken);
        }
    }
}
