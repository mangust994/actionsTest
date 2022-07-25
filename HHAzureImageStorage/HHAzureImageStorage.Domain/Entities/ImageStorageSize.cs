using HHAzureImageStorage.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;


namespace HHAzureImageStorage.Domain.Entities
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ImageStorageSize : IEntity
    {
        public int ImageStorageSizeId { get; set; }

        public ImageVariant imageVariantId { get; set; }

        public string Name { get; set; }

        public string Mnemonic { get; set; }

        public int LongestPixelSize { get; set; }

        public Guid id { get; set; }

        public ImageStorageSize()
        {
            id = Guid.NewGuid();
        }
    }
}
