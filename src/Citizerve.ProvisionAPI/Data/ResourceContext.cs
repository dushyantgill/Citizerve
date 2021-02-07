using Citizerve.ProvisionAPI.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.ProvisionAPI.Data
{
    public class ResourceContext
    {
        private readonly IMongoClient _client = null;
        private readonly IMongoDatabase _database = null;
        private readonly IMongoCollection<Resource> _resources = null;

        public ResourceContext(IDatabaseSettings settings)
        {
            _client = new MongoClient(settings.ConnectionString);
            _database = _client.GetDatabase(settings.DatabaseName);
            _resources = _database.GetCollection<Resource>(settings.ResourceCollectionName);
        }

        public IMongoClient Client
        {
            get
            {
                return _client;
            }
        }

        public IMongoDatabase Database
        {
            get
            {
                return _database;
            }
        }

        public IMongoCollection<Resource> Resources
        {
            get
            {
                return _resources;
            }
        }
    }
}
