namespace HHAzureImageStorage.CosmosRepository.Settings
{
    public class CosmosSettings
    {
        public const string SettingName = "CosmosSettings";

        public string EndPoint { get; set; }

        public string Key { get; set; }

        public string DatabaseName { get; set; }

        public string ImageContainerName { get; set; }

        public string ImageStorageContainerName { get; set; }

        public string ImageStorageAccessUrlContainerName { get; set; }

        public string ImageApplicationRetentionContainerName { get; set; }

        public string ImageStorageSizeContainerName { get; set; }

        public string ImageUploadContainerName { get; set; }

        public string ProcessThumbTrysCountContainerName { get; set; }
    }
}
