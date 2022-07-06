using System;

namespace HHAzureImageStorage.BL.Models.DTOs
{
    public class RebuildThumbnailsWithWatermarkDto
    {
        public string WatermarkImageId { get; set; }

        public int StudioKey { get; set; }

        public string WatermarkMethod { get; set; }

        public int EventKey { get; set; }
    }
}
