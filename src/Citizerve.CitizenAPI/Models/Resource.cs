using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Citizerve.CitizenAPI.Models
{
    public class Resource
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement ("id")]
        public string InternalId { get; set; }

        [BsonElement("resourceId")]
        public string ResourceId { get; set; }

        [BsonElement("citizenId")]
        public string CitizenId { get; set; }

        [BsonElement("tenantId")]
        public string TenantId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }
    }
}