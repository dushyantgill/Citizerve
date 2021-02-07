using Citizerve.ProvisionAPI.Data;
using Citizerve.ProvisionAPI.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.ProvisionAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //ProvisionAPI calls CosmosDB MongoDB
            services.AddSingleton<IDatabaseSettings>(new DatabaseSettings
            {
                //Connection string comes from key vault
                ConnectionString = Configuration["citizerve-cosmosdb-connectionstring-primary"],
                //Rest of the settings come from appSettings.json
                DatabaseName = Configuration.GetSection("DatabaseSettings")["DatabaseName"],
                ResourceCollectionName = Configuration.GetSection("DatabaseSettings")["ResourceCollectionName"]
            });
            services.AddSingleton<IResourceRepository, ResourceRepository>();

            //ProvisionAPI write to Service Bus queues
            services.AddSingleton<IQueueSettings>(new QueueSettings
            {
                //Connection string comes from key vault
                ConnectionString = Configuration["citizerve-servicebus-connectionstring-primary"],
                //Rest of the settings come from appSettings.json
                ProvisionQueueName = Configuration.GetSection("QueueSettings")["ProvisionQueueName"],
            });
            services.AddSingleton<IQueuePublisher, QueuePublisher>();

            services.AddControllers();

            //ProvisionAPI uses Azure Active Directory authentication and authorization
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddInMemoryTokenCaches();

            IdentityModelEventSource.ShowPII = true;

            //ProvisionAPI uses ASP .NET Core API versioning
            services.AddApiVersioning();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
