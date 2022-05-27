using AuthorizationFunc;
using AuthorizationFunc.Clients;
using AuthorizationFunc.Clients.Interfaces;
using AuthorizationFunc.Configs;
using AuthorizationFunc.Parsers;
using AuthorizationFunc.Parsers.Interfaces;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: WebJobsStartup(typeof(Startup))]

namespace AuthorizationFunc
{
    [ExcludeFromCodeCoverage]
    class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services).BuildServiceProvider(true);
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {

            services.AddOptions<TokenSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("AuthorizationFunc").Bind(settings);
            });

            services.AddLogging();
            services.AddScoped<IAuthorizationFuncClient, AuthorizationFuncClient>();
            services.AddScoped<IRedisClient, RedisClient>();
            services.AddScoped<IFormDataParser, FormDataParser>();

            return services;
        }
    }
}
