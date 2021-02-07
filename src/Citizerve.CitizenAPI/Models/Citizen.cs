using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Citizerve.CitizenAPI.Models
{
    public class Citizen
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement ("id")]
        public string InternalId { get; set; }
        
        [BsonElement("citizenId")]
        public string CitizenId { get; set; }

        [BsonElement("tenantId")]
        public string TenantId { get; set; }

        [BsonElement("givenName")]
        public string GivenName { get; set; }

        [BsonElement("surname")]
        public string Surname { get; set; }

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [BsonElement("address")]
        public CitizenAddress Address { get; set; }
    }
}