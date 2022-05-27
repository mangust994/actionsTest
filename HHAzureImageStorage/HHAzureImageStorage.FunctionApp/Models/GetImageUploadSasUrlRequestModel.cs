using System;

namespace HHAzureImageStorage.FunctionApp.Models
{
    public class GetImageUploadSasUrlRequestModel
    {
        public int PhotographerKey { get; set; }

        public int AlbumKey { get; set; }

        public int EventKey { get; set; }

        public string OriginalFileName { get; set; }

        public bool ColorCorrectLevel { get; set; } = false;

        public bool AutoThumbnails { get; set; } = true;

        public string WatermarkImageId { get; set; }

        public string WatermarkMethod { get; set; }

        public DateTime? ExpirationDate { get; set; } = null;

        public bool HiResDownload { get; set; }

        public bool HasTransparentAlphaLayer { get; set; }

    }
}
