using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace HHAzureImageStorage.Domain.Entities
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Image : IEntity
    {
        public Guid id { get; set; }

        public bool HasTransparentAlphaLayer { get; set; }

        public bool ColorCorrectLevel { get; set; }

        public string MimeType { get; set; }

        public string OriginalImageName { get; set; }

        public bool AutoThumbnails { get; set; }

        public string WatermarkImageId { get; set; }

        public string WatermarkMethod { get; set; }

        public int hhihPhotographerKey { get; set; }

        public int hhihEventKey { get; set; }

        public Guid BackupImageGUID { get; set; }

        public int WidthPixels { get; set; }

        public int HeightPixels { get; set; }

        public long SizeInBytes { get; set; }
    }
}
