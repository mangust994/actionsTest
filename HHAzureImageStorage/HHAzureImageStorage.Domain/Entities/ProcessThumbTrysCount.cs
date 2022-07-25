using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace HHAzureImageStorage.Domain.Entities
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProcessThumbTrysCount : IEntity
    {
        public Guid id { get; set; }

        public byte ReTrysCount { get; set; }

        public DateTime ExecutingTime { get; set; }
    }
}
