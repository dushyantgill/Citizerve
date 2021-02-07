using Citizerve.CitizenAPI.Data;
using Citizerve.CitizenAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Citizerve.CitizenAPI
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
            //CitizenAPI calls CosmosDB MongoDB
            services.AddSingleton<IDatabaseSettings>(new DatabaseSettings
            {
                //Connection string comes from key vault
                ConnectionString = Configuration["citizerve-cosmosdb-connectionstring-primary"],
                //Rest of the settings come from appSettings.json
                DatabaseName = Configuration.GetSection("DatabaseSettings")["DatabaseName"],
                CitizenCollectionName = Configuration.GetSection("DatabaseSettings")["CitizenCollectionName"]
            });
            services.AddSingleton<ICitizenRepository, CitizenRepository>();

            //CitizenAPI calls ProvisionAPI using HttpClient + Polly for retry w/ exponential backoff
            services.AddHttpClient<IProvisionService, ProvisionService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());
            services.Configure<ProvisionServiceSettings>(Configuration.GetSection(nameof(ProvisionServiceSettings)));
            services.AddSingleton<IProvisionServiceSettings>(sp => sp.GetRequiredService<IOptions<ProvisionServiceSettings>>().Value);
            services.AddSingleton<IProvisionService, ProvisionService>();

            services.AddControllers();

            //CitizenAPI uses Azure Active Directory authentication and authorization
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddInMemoryTokenCaches();

            IdentityModelEventSource.ShowPII = true;

            //CitizenAPI uses ASP .NET Core API versioning
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
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
