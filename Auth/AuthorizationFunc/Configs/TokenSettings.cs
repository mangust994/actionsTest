using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationFunc.Configs
{
    public class TokenSettings
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Key { get; set; }

        public int Lifetime { get; set; }

        public string URI_HIHH_API { get; set; }

        public string RedisConnectionString { get; set; }
    }
}
