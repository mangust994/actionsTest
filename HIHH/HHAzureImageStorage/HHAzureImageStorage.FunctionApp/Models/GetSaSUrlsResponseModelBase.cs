using System.Collections.Generic;

namespace HHAzureImageStorage.FunctionApp.Models
{
    public class GetSaSUrlsResponseModelBase
    {
        public Dictionary<string, string> SaSUrls { get; set; }        

        public GetSaSUrlsResponseModelBase()
        {
            SaSUrls = new Dictionary<string, string>();
        }
    }
}
