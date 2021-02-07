using Citizerve.SyncWorker.Services;
using Citizerve.SyncWorker.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Citizerve.SyncWorker.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Citizerve.SyncWorker
{
    public class SyncWorker : BackgroundService
    {
        private readonly ILogger<SyncWorker> _logger;
        private readonly IFakeDataRepository _fakeDataRepository;
        private IHttpClientFactory _clientFactory;
        private Int64 _executionCount;
        private AzureADSettings _azureADSettings;
        private CitizenServiceSettings _citizenServiceSettings;
        private Dictionary<string, string> _accessTokens;

        public SyncWorker(ILogger<SyncWorker> logger, IFakeDataRepository fakeDataRepository,
            IHttpClientFactory clientFactory, AzureADSettings azureADSettings, CitizenServiceSettings citizenServiceSettings)
        {
            _logger = logger;
            _fakeDataRepository = fakeDataRepository;
            _clientFactory = clientFactory;
            _azureADSettings = azureADSettings;
            _citizenServiceSettings = citizenServiceSettings;
            _executionCount = 0;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //every 30 minutes, refresh tokens
                if (_executionCount % (30 * 60) == 0)
                {
                    var customers = await _fakeDataRepository.GetCustomers();
                    await GetAccessTokens(customers);
                }

                // every 15 seconds, create 10 citizens
                if (_executionCount % 15 == 0)
                {
                    var customers = await _fakeDataRepository.GetCustomers();
                    var givenNames = await _fakeDataRepository.GetGivenNames();
                    var surnames = await _fakeDataRepository.GetSurnames();
                    var streetNames = await _fakeDataRepository.GetStreetNames();
                    var cities = await _fakeDataRepository.GetCities();

                    for (var count = 0; count < 10; count++)
                    {
                        var customer = customers[new Random().Next(0, customers.Count)];
                        var city = cities[new Random().Next(0, cities.Count)];
                        var streetName = streetNames[new Random().Next(0, streetNames.Count)];

                        var citizen = new Citizen()
                        {
                            CitizenId = Guid.NewGuid().ToString(),
                            GivenName = givenNames[new Random().Next(0, givenNames.Count)].Name,
                            Surname = surnames[new Random().Next(0, surnames.Count)].Name,
                            PhoneNumber = string.Format("({0}) {1}{2}{3} - {4}{5}{6}{7}", city.AreaCode,
                                new Random().Next(0, 9).ToString(), new Random().Next(0, 9).ToString(),
                                new Random().Next(0, 9).ToString(), new Random().Next(0, 9).ToString(),
                                new Random().Next(0, 9).ToString(), new Random().Next(0, 9).ToString(),
                                new Random().Next(0, 9).ToString()),
                            Address = new CitizenAddress()
                            {
                                StreetAddress = string.Format("{0}{1}{2} {3}", new Random().Next(0, 9).ToString(),
                                new Random().Next(0, 9).ToString(), new Random().Next(0, 9).ToString(),
                                streetNames[new Random().Next(0, streetNames.Count)].Name),
                                City = city.CityName,
                                State = city.StateName,
                                PostalCode = city.PostalCode,
                                Country = city.CountryName
                            }
                        };
                        _ = CreateCitizen(customer.TenantId, citizen);
                    }
                }

                // every 5 seconds, execute 100 searches
                if (_executionCount % 5 == 0)
                {
                    var customers = await _fakeDataRepository.GetCustomers();
                    var givenNames = await _fakeDataRepository.GetGivenNames();
                    var surnames = await _fakeDataRepository.GetSurnames();
                    var cities = await _fakeDataRepository.GetCities();

                    for (var count = 0; count < 25; count++)
                    {
                        var customer = customers[new Random().Next(0, customers.Count)];
                        var givenName = givenNames[new Random().Next(0, givenNames.Count)].Name;
                        _ = SearchCitizens(customer.TenantId, givenName, null, null, null, null);
                    }

                    for (var count = 0; count < 25; count++)
                    {
                        var customer = customers[new Random().Next(0, customers.Count)];
                        var surname = surnames[new Random().Next(0, surnames.Count)].Name;
                        _ = SearchCitizens(customer.TenantId, surname, null, null, null, null);
                    }

                    for (var count = 0; count < 25; count++)
                    {
                        var customer = customers[new Random().Next(0, customers.Count)];
                        var city = cities[new Random().Next(0, cities.Count)];
                        _ = SearchCitizens(customer.TenantId, null, city.PostalCode, null, null, null);
                    }

                    for (var count = 0; count < 25; count++)
                    {
                        var customer = customers[new Random().Next(0, customers.Count)];
                        var city = cities[new Random().Next(0, cities.Count)];
                        _ = SearchCitizens(customer.TenantId, null, null, city.CityName, city.StateName, city.CountryName);
                    }
                }

                // every minute, delete 10 citizens
                if (_executionCount % 60 == 0)
                {
                    var customers = await _fakeDataRepository.GetCustomers();

                    for (var count = 0; count < 10; count++)
                    {
                        var citizens = await GetCitizens(customers[new Random().Next(0, customers.Count)].TenantId);
                        var citizen = citizens[new Random().Next(0, citizens.Count)];
                        _ = DeleteCitizen(citizen.TenantId, citizen.CitizenId);
                    }
                }

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _executionCount++;
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task GetAccessTokens(List<Customer> customers)
        {
            _accessTokens = new Dictionary<string, string>();

            foreach (var customer in customers)
            {
                using (var httpClient = _clientFactory.CreateClient())
                {
                    string postUrl = String.Format(_azureADSettings.TokenUrl, customer.TenantId);
                    var urlEncodedContent = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&resource={2}",
                        Uri.EscapeDataString(_azureADSettings.ClientId), Uri.EscapeDataString(_azureADSettings.ClientSecret),
                        Uri.EscapeDataString(_azureADSettings.Resource));
                    var postContent = new StringContent(urlEncodedContent, Encoding.UTF8, "application/x-www-form-urlencoded");

                    using (var response = await httpClient.PostAsync(postUrl, postContent))
                    {
                        string tokenResponse = await response.Content.ReadAsStringAsync();
                        var accessToken = JsonConvert.DeserializeObject<AzureADTokenResponse>(tokenResponse).AccessToken;

                        _accessTokens.Add(customer.TenantId, accessToken);
                    }
                }
            }
        }

        private async Task CreateCitizen(string tenantId, Citizen citizen)
        {
            var accessToken = _accessTokens[tenantId];

            using (var httpClient = _clientFactory.CreateClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(citizen), Encoding.UTF8, "application/json");
                string postUrl = String.Format(_citizenServiceSettings.Url + "?api-version={0}", _citizenServiceSettings.ApiVersion);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                await httpClient.PostAsync(postUrl, content);
            }
        }

        private async Task SearchCitizens(string tenantId, string name, string postalCode, string city, string state, string country)
        {
            var accessToken = _accessTokens[tenantId];

            using (var httpClient = _clientFactory.CreateClient())
            {
                string getUrl = String.Format(_citizenServiceSettings.Url +
                    "/search?api-version={0}&name={1}&postalCode={2}&city={3}&state={4}&country={5}",
                    _citizenServiceSettings.ApiVersion, name, postalCode, city, state, country);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                await httpClient.GetAsync(getUrl);
            }
        }

        private async Task<List<Citizen>> GetCitizens(string tenantId)
        {
            var accessToken = _accessTokens[tenantId];
            List<Citizen> citizens = null;

            using (var httpClient = _clientFactory.CreateClient())
            {
                string getUrl = String.Format(_citizenServiceSettings.Url + "?api-version={0}", _citizenServiceSettings.ApiVersion);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var getResponse = await httpClient.GetAsync(getUrl);
                if (getResponse.IsSuccessStatusCode)
                {
                    string citizenResponse = await getResponse.Content.ReadAsStringAsync();
                    citizens = JsonConvert.DeserializeObject<List<Citizen>>(citizenResponse);
                }
            }

            return citizens;
        }

        private async Task DeleteCitizen(string tenantId, string citizenId)
        {
            var accessToken = _accessTokens[tenantId];

            using (var httpClient = _clientFactory.CreateClient())
            {
                string deleteUrl = String.Format(_citizenServiceSettings.Url + "/{0}?api-version={1}", citizenId, _citizenServiceSettings.ApiVersion);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                await httpClient.DeleteAsync(deleteUrl);
            }
        }
    }
}
