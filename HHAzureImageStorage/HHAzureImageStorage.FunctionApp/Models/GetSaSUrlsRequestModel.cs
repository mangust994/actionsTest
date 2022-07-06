using System;
using System.Collections.Generic;
using HHAzureImageStorage.Domain.Enums;

namespace HHAzureImageStorage.FunctionApp.Models
{
    public class GetSaSUrlsRequestModel
    {
        public Guid ImageIdGuid { get; set; }

        public List<ImageVariant> ImageVariantIds { get; set; }

        public bool WantImageInfo { get; set; }

        public int ExpireInDays { get; set; }
    }
}
