using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationFunc.Clients.Interfaces
{
    public interface IRedisClient
    {
        Task<string> GetCahedToken(string key);

        Task SetCahedToken(string key, string token);
    }
}
