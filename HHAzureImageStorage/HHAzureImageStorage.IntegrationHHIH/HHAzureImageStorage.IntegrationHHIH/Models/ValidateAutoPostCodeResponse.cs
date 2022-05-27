namespace HHAzureImageStorage.IntegrationHHIH.Models
{
    public class ValidateAutoPostCodeResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public int EventKey { get; set; }

        public string EventType { get; set; }

        public string EventName { get; set; }

        public string PublishDate { get; set; }

        public string ExpirationDate { get; set; }

        public string EventUid { get; set; }

        public string StudioName { get; set; }

        public string EventUrl { get; set; }

        public bool IsKoEvent { get; set; }

        public bool HiresImagesRequired { get; set; }
    }
}
