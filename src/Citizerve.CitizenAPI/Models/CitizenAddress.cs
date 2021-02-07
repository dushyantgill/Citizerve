using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Citizerve.CitizenAPI.Models
{
    public class CitizenAddress
    {
        [BsonElement("streetAddress")]
        public string StreetAddress { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("state")]
        public string State { get; set; }

        [BsonElement("postalCode")]
        public string PostalCode { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }
    }
}