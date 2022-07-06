using HHAzureImageStorage.CosmosRepository.Interfaces;
using HHAzureImageStorage.CosmosRepository.Containers;
using HHAzureImageStorage.CosmosRepository.Repositories;
using HHAzureImageStorage.CosmosRepository.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HHAzureImageStorage.DAL.Interfaces;
using Microsoft.Azure.Cosmos;
using System;

namespace HHAzureImageStorage.CosmosRepository
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCosmosRepository(
          this IServiceCollection services,
          IConfiguration configuration)
        {
            CosmosSettings cosmosSettings = new CosmosSettings();
            configuration.GetSection(CosmosSettings.SettingName).Bind(cosmosSettings);

            CosmosClient cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("COSMOS_END_POINT"),
                       Environment.GetEnvironmentVariable("COSMOS_KEY"));

            services.AddSingleton<CosmosSettings>(cosmosSettings);
            services.AddSingleton<CosmosClient>(cosmosClient);

            services.AddSingleton<IImageCosmosContext, ImageCosmosContext>();
            services.AddSingleton<IImageStorageCosmosContext, ImageStorageCosmosContext>();
            services.AddSingleton<IImageStorageAccessUrlCosmosContext, ImageStorageAccessUrlCosmosContext>();
            services.AddSingleton<IImageApplicationRetentionCosmosContext, ImageApplicationRetentionCosmosContext>();
            services.AddSingleton<IImageStorageSizeCosmosContext, ImageStorageSizeCosmosContext>();
            services.AddSingleton<IImageUploadCosmosContext, ImageUploadCosmosContext>();

            services.AddScoped<IImageRepository, ImageCosmosRepository>();
            services.AddScoped<IImageStorageRepository, ImageStorageCosmosRepository>();
            services.AddScoped<IImageStorageAccessUrlRepository, ImageStorageAccessUrlCosmosRepository>();
            services.AddScoped<IImageApplicationRetentionRepository, ImageApplicationRetentionCosmosRepository>();
            services.AddScoped<IImageStorageSizeRepositoty, ImageStorageSizeCosmosRepositoty>();
            services.AddScoped<IImageUploadRepository, ImageUploadCosmosRepository>();

            return services;
        }
    }
}
