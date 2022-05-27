using System;

namespace HHAzureImageStorage.IntegrationHHIH.Models
{
    public class AddImageInfoResponseModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string WatermarkImageId { get; set; }

        public string WatermarkMethod { get; set; }

        public bool AutoThumbnails { get; set; }

        public int HHIHPhotographerKey { get; set; }

        public int HHIHEventKey { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
