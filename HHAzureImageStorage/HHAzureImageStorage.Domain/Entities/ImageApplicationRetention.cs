using System;

namespace HHAzureImageStorage.Domain.Entities
{
    public class ImageApplicationRetention : IEntity
    {
        public Guid id { get; set; }

        public string sourceApplicationName { get; set; }

        public int? sourceApplicationReferenceId { get; set; }

        public DateTime? expirationDate { get; set; }
    }
}
