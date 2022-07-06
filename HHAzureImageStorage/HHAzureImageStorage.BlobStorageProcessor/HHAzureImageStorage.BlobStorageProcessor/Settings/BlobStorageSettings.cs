namespace HHAzureImageStorage.BlobStorageProcessor.Settings
{
    public class BlobStorageSettings
    {
        public const string SettingName = "BlobStorageSettings";

        public string ConnectionStringTemp { get; set; }

        public string ConnectionStringMain { get; set; }

        public string ConnectionStringThumbnail { get; set; }

        public string AccountNameTemp { get; set; }

        public string AccountNameMain { get; set; }

        public string AccountNameThumbnail { get; set; }

        public string AccountKeyTemp { get; set; }

        public string AccountKeyMain { get; set; }

        public string AccountKeyThumbnail { get; set; }

        public string ContainerNameTemp { get; set; }

        public string ContainerNameMain { get; set; }

        public string ContainerNameThumbnail { get; set; }
        
        public int UploadsContainerUrlExpireMinutes { get; set; }

        public int SasUrlExpireDatetimeDeltaMinutes { get; set; }

        public int SasUrlExpireDateTimeDays { get; set; }
    }
}
