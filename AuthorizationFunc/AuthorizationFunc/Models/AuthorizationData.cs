using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationFunc.Models
{
    public class AuthorizationData
    {
        public string SecurityKey { get; set; }

        public string EventKey { get; set; }

        public string EventUid { get; set; }
    }
}
