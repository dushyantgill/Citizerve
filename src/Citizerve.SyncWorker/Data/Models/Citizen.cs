using Newtonsoft.Json;

namespace Citizerve.SyncWorker.Models
{
    public class Citizen
    {
        [JsonProperty(PropertyName = "citizenId")]
        public string CitizenId { get; set; }

        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "surname")]
        public string Surname { get; set; }

        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "address")]
        public CitizenAddress Address { get; set; }
    }
}