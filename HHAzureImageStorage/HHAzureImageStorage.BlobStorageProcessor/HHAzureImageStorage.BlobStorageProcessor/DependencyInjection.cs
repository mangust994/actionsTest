using HHAzureImageStorage.BlobStorageProcessor.Interfaces;
using HHAzureImageStorage.BlobStorageProcessor.Settings;
using HHAzureImageStorage.BlobStorageProcessor.Utilities;
using HHAzureImageStorage.Core.Interfaces.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HHAzureImageStorage.BlobStorageProcessor
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAzureBlobStorage(
          this IServiceCollection services,
          IConfiguration configuration)
        {
            BlobStorageSettings blobStorageSettings = new BlobStorageSettings();

            configuration.GetSection(BlobStorageSettings.SettingName).Bind(blobStorageSettings);

            services.AddSingleton<BlobStorageSettings>(blobStorageSettings);

            services.AddSingleton<IStorageHelper, BlobStorageHelper>();

            services.AddSingleton<IStorageProcessor, AzureBlobStorageProcessor>();

            return services;
        }
    }
}
