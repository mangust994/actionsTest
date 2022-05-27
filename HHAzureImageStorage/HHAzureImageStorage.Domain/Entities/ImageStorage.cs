using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Domain.Entities
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ImageStorage : IEntity
    {
        public Guid id { get; set; }

        public Guid imageId { get; set; }

        public ImageVariant imageVariantId { get; set; }

        public ImageStatus Status { get; set; }

        public string StorageAccount { get; set; }

        public string Container { get; set; }

        public string BlobName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int WidthPixels { get; set; }

        public int HeightPixels { get; set; }

        public int SizeInBytes { get; set; }
    }
}
