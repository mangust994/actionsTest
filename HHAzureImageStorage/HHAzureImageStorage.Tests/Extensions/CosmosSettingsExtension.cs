using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Extensions.Configuration;

namespace HHAzureImageStorage.Tests.Extensions
{
    internal class CosmosSettingsExtension
    {
        private static IConfigurationRoot _configuration;
        private static CosmosSettings _cosmosSettings;

        public static CosmosSettings GetTestCosmosSettings()
        {
            if (_cosmosSettings == null)
            {
                if (_configuration == null)
                {
                    _configuration = new ConfigurationBuilder()
                     .AddJsonFile("test-config.json")
                     .Build();
                }

                _cosmosSettings = new CosmosSettings
                {
                    DatabaseName = _configuration["COSMOS_DatabaseName"],
                    EndPoint = _configuration["COSMOS_END_POINT"],
                    Key = _configuration["COSMOS_KEY"],
                    ImageContainerName = _configuration["COSMOS_ImageContainerName"],
                    ImageStorageContainerName = _configuration["COSMOS_ImageStorageContainerName"],
                    ImageStorageAccessUrlContainerName = _configuration["COSMOS_ImageStorageAccessUrlContainerName"],
                    ImageApplicationRetentionContainerName = _configuration["COSMOS_ImageApplicationRetentionContainerName"],
                    ImageStorageSizeContainerName = _configuration["COSMOS_ImageStorageSizeContainerName"],
                    ImageUploadContainerName = _configuration["COSMOS_ImageUploadContainerName"]
                };
            }

            return _cosmosSettings;
        }
    }
}
