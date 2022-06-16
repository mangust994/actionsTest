using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.Domain.Entities
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ImageStorageAccessUrl : IEntity
    {
        public Guid id { get; set; }

        public Guid imageId { get; set; }

        public ImageVariant imageVariantId { get; set; }

        public string SaSUrl { get; set; }

        public DateTime SasUrlExpireDatetime { get; set; }
    }
}
