using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using HHAzureImageStorage.CosmosRepository;
using HHAzureImageStorage.BlobStorageProcessor;
using Microsoft.Extensions.Configuration;
using HHAzureImageStorage.BL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using HHAzureImageStorage.BL.Validators;
using HHAzureImageStorage.IntegrationHHIH;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using System;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.BL.Services;

namespace HHAzureImageStorage.FunctionApp
{
    public class Program
    {
        private static IConfiguration config;

        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureFunctionsWorkerDefaults(workerApplication =>
                {
                    // Register our custom middleware with the worker
                    workerApplication.UseNewtonsoftJson();
                })
                //.ConfigureOpenApi()
                .ConfigureAppConfiguration(c =>
                {
#if DEBUG
                    c.AddJsonFile("local.settings.json", false, false);
                    
#endif
                    c.AddEnvironmentVariables();

                    config = c.Build();
                })
                .ConfigureServices(services =>
                {
                    //services.AddLogging();

                    services.AddAzureAppConfiguration();

                    services.AddSingleton<IImageResizer, ImageResizerMagickImage>();
                    services.AddSingleton<IUploadFileValidator, UploadFileValidator>();
                    services.AddSingleton<IUploadFileHelper, UploadFileHelper>();
                    services.AddSingleton<IHttpHelper, HttpHelper>();
                    services.AddScoped<IImageService, ImageService>();

                    services.AddCosmosRepository(config);
                    services.AddAzureBlobStorage(config);

                    services.AddHttpClient<HHIHHttpClient>();

                    services.AddAzureClients(bundler =>
                    {
                        bundler.AddClient<QueueClient, QueueClientOptions>((options, _, _) =>
                       {
                           options.MessageEncoding = QueueMessageEncoding.Base64;

                           var conString = Environment.GetEnvironmentVariable("QUEUE_CON_STR");
                           var queueName = Environment.GetEnvironmentVariable("QUEUE_NAME");

                           return new QueueClient(conString, queueName, options);
                       });
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}