using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthorizationFunc.Parsers.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationFunc.Parsers
{
    public class FormDataParser : IFormDataParser
    {
        public string ParseValue(string key, string body)
        {
            string[] splitedKeys = body.Split("----------------------------");
            var withNeededValue = splitedKeys.Where(x => x.Contains(key)).FirstOrDefault();
            string[] withNeededValueAdditionalSplit = withNeededValue.Split("Disposition: form-data; name=");
            var pairAfterSeparate = withNeededValueAdditionalSplit.Where(x => x.Contains(key)).FirstOrDefault();
            return pairAfterSeparate.Split("\r\n").Where(x => !x.Contains(key) && !x.IsNullOrEmpty()).FirstOrDefault();
        }
    }
}
