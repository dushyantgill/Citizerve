using Citizerve.ProvisionAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.ProvisionAPI.Data
{
    public class ResourceRepository : IResourceRepository
    {
        private readonly ResourceContext _context = null;

        public ResourceRepository(IDatabaseSettings settings)
        {
            _context = new ResourceContext(settings);
        }

        public async Task<IEnumerable<Resource>> GetResources(string tenantId)
        {
            var tenantIdFilter = Builders<Resource>.Filter.Eq(r => r.TenantId, tenantId);

            return await _context.Resources.Find(tenantIdFilter).Limit(100).ToListAsync();
        }

        public async Task<Resource> GetResource(string resourceId, string tenantId)
        {
            var tenantIdFilter = Builders<Resource>.Filter.Eq(r => r.TenantId, tenantId);
            var resourceIdFilter = Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId);

            return await _context.Resources.Find(tenantIdFilter & resourceIdFilter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Resource>> GetResourcesByCitizenId(string citizenId, string tenantId)
        {
            var tenantIdFilter = Builders<Resource>.Filter.Eq(r => r.TenantId, tenantId);
            var citizenIdFilter = Builders<Resource>.Filter.Eq(r => r.CitizenId, citizenId);

            return await _context.Resources.Find(tenantIdFilter & citizenIdFilter).ToListAsync();
        }

        public async Task AddResource(Resource resource, string tenantId)
        {
            var tenantIdFilter = Builders<Resource>.Filter.Eq(r => r.TenantId, tenantId);
            var resourceIdFilter = Builders<Resource>.Filter.Eq(r => r.ResourceId, resource.ResourceId);

            var result = await _context.Resources.Find(tenantIdFilter & resourceIdFilter).FirstOrDefaultAsync();

            if (result == null)
            {
                await _context.Resources.InsertOneAsync(resource);
            }
            else
                throw new DuplicateNameException();
        }

        public async Task<bool> RemoveResource(string resourceId, string tenantId)
        {
            var tenantIdFilter = Builders<Resource>.Filter.Eq(r => r.TenantId, tenantId);
            var resourceIdFilter = Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId);

            DeleteResult actionResult = await _context.Resources.DeleteOneAsync(tenantIdFilter & resourceIdFilter);

            return actionResult.IsAcknowledged && actionResult.DeletedCount > 0;
        }

        public async Task<bool> ReplaceResource(string resourceId, Resource resource, string tenantId)
        {
            var tenantIdFilter = Builders<Resource>.Filter.Eq(r => r.TenantId, tenantId);
            var resourceIdFilter = Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId);

            ReplaceOneResult actionResult = await _context.Resources.ReplaceOneAsync(tenantIdFilter & resourceIdFilter, resource);

            return actionResult.IsAcknowledged && actionResult.ModifiedCount > 0;
        }
    }
}