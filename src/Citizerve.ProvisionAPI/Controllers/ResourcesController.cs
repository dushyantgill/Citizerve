using Citizerve.ProvisionAPI.Data;
using Citizerve.ProvisionAPI.Messaging;
using Citizerve.ProvisionAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Citizerve.ProvisionAPI.Controllers
{
    [Authorize (Roles = "Global.Resources.Read")]
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IQueuePublisher _queuePublisher;

        public ResourcesController(IResourceRepository resourceRepository, IQueuePublisher queuePublisher)
        {
            _resourceRepository = resourceRepository;
            _queuePublisher = queuePublisher;
        }

        // GET /resources/
        [HttpGet]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetV1()
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var results = await _resourceRepository.GetResources(tenantId);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // GET /resources/739ad8de-7b3b-45c1-a90c-697ef16317ce/
        [HttpGet("{resourceId}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Resource>> GetV1(string resourceId)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
            
            try
            {
                var result = await _resourceRepository.GetResource(resourceId, tenantId);
                if (result == null) return NotFound(); 
                return Ok(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // GET /resources/search?citizen-id=739ad8de-7b3b-45c1-a90c-697ef16317ce
        [HttpGet("search")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Resource[]>> SearchV1(string citizenId)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var results = await _resourceRepository.GetResourcesByCitizenId(citizenId, tenantId);
                if (!results.Any()) return NotFound();
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // POST /resources/
        [Authorize (Roles = "Global.Resources.Write")]
        [HttpPost]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Resource>> CreateV1(Resource resource)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            #region Field Validation
            if (String.IsNullOrEmpty(resource.Name)) return BadRequest("Oops! Sorry, can't create a resource without name.");
            if (String.IsNullOrEmpty(resource.Status)) return BadRequest("Oops! Sorry, can't create a resource  without status."); 
            if (String.IsNullOrEmpty(resource.CitizenId)) return BadRequest("Oops! Sorry, can't create a resource without a citizenId."); 
            if (!String.IsNullOrEmpty(resource.TenantId) && !resource.TenantId.Equals(tenantId, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest("Oops! Sorry, can't create a resource in a tenant other than your own.");
            if (String.IsNullOrEmpty(resource.ResourceId)) resource.ResourceId = Guid.NewGuid().ToString();
            resource.TenantId = tenantId;
            #endregion

            try
            {   
                await _resourceRepository.AddResource(resource, tenantId);

                //after creating resource, write resource message to provision queue
                await _queuePublisher.PublishProvisionResource(resource);

                var createdResource = await _resourceRepository.GetResource(resource.ResourceId, tenantId);
                if (createdResource != null) return Created($"/Resources/{resource.ResourceId}", createdResource);
                //Return failure if the created resource can't be retrieved successfully
                else return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something might have gone wrong.");
            }
            catch (DuplicateNameException)
            {
                return this.StatusCode(StatusCodes.Status409Conflict, "Oops! Sorry, a resource with that resourceId already exists.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // DELETE /resources/739ad8de-7b3b-45c1-a90c-697ef16317ce/
        [Authorize (Roles = "Global.Resources.Write")]
        [HttpDelete("{resourceId}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult> DeleteV1(string resourceId)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var resource = await _resourceRepository.GetResource(resourceId, tenantId);
                if (resource == null) return NotFound("Oops! Sorry, can't find that resource.");

                var deleteAccepted = await _resourceRepository.RemoveResource(resourceId, tenantId);

                if (deleteAccepted) return this.StatusCode(StatusCodes.Status202Accepted);
                else return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something might have gone wrong.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // PUT /resources/739ad8de-7b3b-45c1-a90c-697ef16317ce/
        [Authorize (Roles = "Global.Resource.Write")]
        [HttpPut("{resourceId}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Resource>> PutV1(string resourceId, Resource resourceUpdates)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var oldResource = await _resourceRepository.GetResource(resourceId, tenantId);
                if (oldResource == null) return NotFound("Oops! Sorry, can't find that resource.");

                #region Field Validation
                if (!String.IsNullOrEmpty(resourceUpdates.InternalId)) return BadRequest("Oops! Sorry, can't update internalId.");
                if (!String.IsNullOrEmpty(resourceUpdates.ResourceId)) return BadRequest("Oops! Sorry, can't update resourceId.");
                if (!String.IsNullOrEmpty(resourceUpdates.CitizenId)) return BadRequest("Oops! Sorry, can't update citizenId.");
                if (!String.IsNullOrEmpty(resourceUpdates.TenantId)) return BadRequest("Oops! Sorry, can't update tenantId.");
                if (!String.IsNullOrEmpty(resourceUpdates.Name)) oldResource.Name = resourceUpdates.Name;
                if (!String.IsNullOrEmpty(resourceUpdates.Status)) oldResource.Status = resourceUpdates.Status;
                #endregion

                var updateAccepted = await _resourceRepository.ReplaceResource(resourceId, oldResource, tenantId);

                if (updateAccepted)
                {
                    //return updated resource
                    var result = await _resourceRepository.GetResource(resourceId, tenantId);
                    return Ok(oldResource);
                }
                else return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something might have gone wrong.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }
    }
}
