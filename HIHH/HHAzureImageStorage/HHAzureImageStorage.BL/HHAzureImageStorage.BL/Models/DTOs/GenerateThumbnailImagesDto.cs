using System;

namespace HHAzureImageStorage.BL.Models.DTOs
{
    public class GenerateThumbnailImagesDto
    {
        public Guid ImageId { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }

        public string OriginalFileName { get; set; }

        public bool AutoThumbnails { get; set; }

        public string WatermarkMethod { get; set; }

        public string WatermarkImageId { get; set; }
    }
}
