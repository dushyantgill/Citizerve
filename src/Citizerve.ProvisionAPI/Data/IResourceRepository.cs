using Citizerve.ProvisionAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.ProvisionAPI.Data
{
    public interface IResourceRepository
    {
        Task<IEnumerable<Resource>> GetResources(string tenantId);
        Task<Resource> GetResource(string resourceId, string tenantId);
        Task<IEnumerable<Resource>> GetResourcesByCitizenId(string citizenId, string tenantId);
        Task AddResource(Resource resource, string tenantId);
        Task<bool> RemoveResource(string resourceId, string tenantId);
        Task<bool> ReplaceResource(string resourceId, Resource resource, string tenantId);
    }
}