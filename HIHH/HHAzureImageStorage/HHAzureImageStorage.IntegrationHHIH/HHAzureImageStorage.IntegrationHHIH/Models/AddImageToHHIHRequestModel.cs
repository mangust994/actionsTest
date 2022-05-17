﻿using System.Collections.Generic;

namespace HHAzureImageStorage.IntegrationHHIH.Models
{
    public class AddImageToHHIHRequestModel
    {
        public string SecurityKey { get; set; }

        public string GroupName { get; set; }

        public string Password { get; set; }

        public bool FreeHiResDownload { get; set; }

        public bool AddIfAlreadyExists { get; set; }

        public bool OverwriteExistingImages { get; set; }

        public string OriginalFileName { get; set; }

        public string EmailAddress { get; set; }

        public string CellNumber { get; set; }

        public string CloudImageId { get; set; }

        public List<string> AdditionalEmailAddresses { get; set; }

        public List<string> AdditionalCellNumbers { get; set; }

        public string AutoPostCode { get; set; }

        public bool IsColorCorrected { get; set; }

        public bool HasTransparentAlphaLayer { get; set; }

        public string EventKey { get; set; }

        public string EventUid { get; set; }
    }
}
