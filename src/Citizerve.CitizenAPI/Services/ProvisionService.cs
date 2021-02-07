using Citizerve.CitizenAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging.Configuration;

namespace Citizerve.CitizenAPI.Services
{
    public class ProvisionService : IProvisionService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _url;
        private readonly string _apiVersion;

        public ProvisionService(IHttpClientFactory clientFactory, IProvisionServiceSettings provisionServiceSettings)
        {
            _clientFactory = clientFactory;
            _url = provisionServiceSettings.Url;
            _apiVersion = provisionServiceSettings.ApiVersion;
        }

        public async Task ProvisionDefaultResource(Citizen citizen, string authorizeHeader)
        {
            var resource = new Resource()
            {
                TenantId = citizen.TenantId,
                CitizenId = citizen.CitizenId,
                Name = "Phone Authentication",
                Status = "To Be Assigned"
            };

            using (var httpClient = _clientFactory.CreateClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(resource), Encoding.UTF8, "application/json");
                string postUrl = String.Format(_url + "?api-version={0}", _apiVersion);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizeHeader);

                await httpClient.PostAsync(postUrl, content);
            }
        }

        public async Task DeprovisionAllResources(Citizen citizen, string authorizeHeader)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {
                string getUrl = String.Format(_url + "/search?api-version={0}&citizenId={1}", _apiVersion, citizen.CitizenId);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizeHeader);

                var getResponse = await httpClient.GetAsync(getUrl);
                if (getResponse.IsSuccessStatusCode)
                {
                    string resourcesResponse = await getResponse.Content.ReadAsStringAsync();
                    var resources = JsonConvert.DeserializeObject<List<Resource>>(resourcesResponse);

                    foreach (var resource in resources)
                    {
                        string deleteUrl = String.Format(_url + "/{0}?api-version={1}", resource.ResourceId, _apiVersion);
                        await httpClient.DeleteAsync(deleteUrl);
                    }
                }
            }
        }
    }
}
