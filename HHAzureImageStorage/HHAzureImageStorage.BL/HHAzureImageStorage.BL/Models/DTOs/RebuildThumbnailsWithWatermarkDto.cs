using System;

namespace HHAzureImageStorage.BL.Models.DTOs
{
    public class RebuildThumbnailsWithWatermarkDto
    {
        public string ImageIdStringValue { get; set; }

        public Guid ImageId { get; set; }

        public int StudioKey { get; set; }

        public string WatermarkMethod { get; set; }
    }
}
