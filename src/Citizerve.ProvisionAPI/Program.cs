using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.ProvisionAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //ProvisionAPI reads CosmosDB and ServiceBus connection strings from KeyVault
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    //KeyVaultEndpoint comes from appSettings.json
                    var keyVaultEndpoint = builder.Build()["KeyVaultEndpoint"];
                    var credential = new DefaultAzureCredential();
                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                        builder.AddAzureKeyVault(new System.Uri(keyVaultEndpoint), credential);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
