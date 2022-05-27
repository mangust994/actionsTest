namespace HHAzureImageStorage.FunctionApp.Models
{
    public class GetSaSUrlsResponseModelExtended : GetSaSUrlsResponseModelBase
    {
        public bool HasTransparentAlphaLayer { get; set; }

        public bool ColorCorrectLevel { get; set; }

        public string MimeType { get; set; }

        public string OriginalImageName { get; set; }

        public GetSaSUrlsResponseModelExtended(): base()
        {
            
        }
    }
}
