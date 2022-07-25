using HHAzureImageStorage.BL.Services;
using HHAzureImageStorage.BL.Utilities;
using HHAzureImageStorage.BL.Validators;
using HHAzureImageStorage.BlobStorageProcessor;
using HHAzureImageStorage.Core.Interfaces.Processors;
using HHAzureImageStorage.CosmosRepository;
using HHAzureImageStorage.FunctionApp.Helpers;
using HHAzureImageStorage.IntegrationHHIH;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

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

                    //services.AddSingleton<IImageResizer, ImageResizerMagickImage>();//IImageResizer
                    services.AddSingleton<IImageResizer, ImageSharpResizer>();
                    services.AddSingleton<IUploadFileValidator, UploadFileValidator>();
                    services.AddSingleton<IUploadFileHelper, UploadFileHelper>();
                    services.AddSingleton<IHttpHelper, HttpHelper>();
                    services.AddScoped<IImageService, ImageService>();
                    services.AddScoped<IQueueMessageService, AzurServiceBusQueueMessageService>();

                    services.AddCosmosRepository(config);
                    services.AddAzureBlobStorage(config);

                    services.AddHttpClient<HHIHHttpClient>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}