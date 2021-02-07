using Azure.Identity;
using Citizerve.SyncWorker.Data;
using Citizerve.SyncWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.SyncWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //CitizenSync reads client secret of its Azure AD app and SQLDB connection string from KeyVault
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    //KeyVaultEndpoint comes from appSettings.json
                    var keyVaultEndpoint = builder.Build()["KeyVaultEndpoint"];
                    var credential = new DefaultAzureCredential();
                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                        builder.AddAzureKeyVault(new System.Uri(keyVaultEndpoint), credential);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    //CitizenSync authenticates with Azure AD using client credentials
                    services.AddSingleton<AzureADSettings>(new AzureADSettings
                    {
                        //Azure AD ClientId, Resource and TokenUrl come from appSettings.json
                        ClientId = configuration.GetSection("AzureAd")["ClientId"],
                        TokenUrl = configuration.GetSection("AzureAd")["TokenUrl"],
                        Resource = configuration.GetSection("AzureAd")["Resource"],
                        //ClientSecret comes from key vault
                        ClientSecret = configuration["citizerve-syncworker-azuread-clientsecret-primary"]
                    });

                    //CitizenSync calls SQLDB to read fake name and address data
                    var optionsBuilder = new DbContextOptionsBuilder<FakeDataContext>();
                    optionsBuilder.UseSqlServer(configuration["citizerve-sqldb-connectionstring-primary"]);
                    services.AddSingleton<FakeDataContext>(s => new FakeDataContext(optionsBuilder.Options));
                    services.AddSingleton<IFakeDataRepository, FakeDataRepository>();

                    services.AddHttpClient();

                    //CitizenSync calls CitizenAPI to create, delete, and search citizens
                    CitizenServiceSettings citizenServiceSettings = configuration.GetSection("CitizenServiceSettings").Get<CitizenServiceSettings>();
                    services.AddSingleton(citizenServiceSettings);

                    services.AddHostedService<SyncWorker>();
                });
    }
}
