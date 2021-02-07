using Azure.Identity;
using Citizerve.ProvisionWorker.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.ProvisionWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //Provisioning reads ServiceBus connection string from KeyVault
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
                    //Provisioning reads from Service Bus queue
                    services.AddSingleton<IQueueSettings>(new QueueSettings
                    {
                        //Connection string comes from key vault
                        ConnectionString = configuration["citizerve-servicebus-connectionstring-primary"],
                        //Queue name come from appSettings.json
                        ProvisionQueueName = configuration.GetSection("QueueSettings")["ProvisionQueueName"]
                    });
                    services.AddHostedService<ProvisionWorker>();
                });
    }
}
