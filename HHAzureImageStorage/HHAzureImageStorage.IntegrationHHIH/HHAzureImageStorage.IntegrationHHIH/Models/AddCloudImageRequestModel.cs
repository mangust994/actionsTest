namespace HHAzureImageStorage.IntegrationHHIH.Models
{
    public class AddCloudImageRequestModel
    {
        public int PhotographerKey { get; set; }

        public int EventKey { get; set; }

        public string OriginalFileName { get; set; }

        public string CloudImageId { get; set; }

        public bool HasTransparentAlphaLayer { get; set; }

        public bool HiResDownload { get; set; }

        public int AlbumKey { get; set; }

        public string SecurityKey { get; set; }
        public bool AutoThumbnails { get; set; }
    }
}
