using Newtonsoft.Json;

namespace Citizerve.SyncWorker.Models
{
    public class CitizenAddress
    {
        [JsonProperty(PropertyName = "streetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }
}