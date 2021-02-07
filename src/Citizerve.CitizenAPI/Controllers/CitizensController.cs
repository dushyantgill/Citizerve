using Citizerve.CitizenAPI.Data;
using Citizerve.CitizenAPI.Services;
using Citizerve.CitizenAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Citizerve.CitizenAPI.Controllers
{
    [Authorize (Roles = "Global.Citizens.Read")]
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class CitizensController : ControllerBase
    {
        private readonly ICitizenRepository _citizenRepository;
        private readonly IProvisionService _provisionService;

        public CitizensController(ICitizenRepository citizenRepository, IProvisionService provisionService)
        {
            _citizenRepository = citizenRepository;
            _provisionService = provisionService;
        }

        // GET /citizens/
        [HttpGet]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetV1()
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var results = await _citizenRepository.GetCitizens(tenantId);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // GET /citizens/739ad8de-7b3b-45c1-a90c-697ef16317ce/
        [HttpGet("{citizenId}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Citizen>> GetV1(string citizenId)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
            
            try
            {
                var result = await _citizenRepository.GetCitizen(citizenId, tenantId);
                if (result == null) { return NotFound(); }
                return Ok(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // GET /citizens/search?name=gill&postalCode=98119&city=seattle&state=wa&country=us
        [HttpGet("search")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Resource[]>> SearchV1(string name, string postalCode, string city, string state, string country)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var results = await _citizenRepository.SearchCitizens(tenantId, name, postalCode, city, state, country);
                if (!results.Any()) return NotFound();
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // POST /citizens/
        [Authorize (Roles = "Global.Citizens.Write")]
        [HttpPost]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Citizen>> CreateV1(Citizen citizen)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            #region Field Validation
            if (String.IsNullOrEmpty(citizen.GivenName)) return BadRequest("Oops! Sorry, can't create a citizen without givenName.");
            if (String.IsNullOrEmpty(citizen.Surname)) return BadRequest("Oops! Sorry, can't create a citizen without surname."); 
            if (citizen.Address == null) return BadRequest("Oops! Sorry, can't create a citizen without address."); 
            if (String.IsNullOrEmpty(citizen.Address.StreetAddress)) return BadRequest("Oops! Sorry, can't create a citizen without address.streetAddress."); 
            if (String.IsNullOrEmpty(citizen.Address.City)) return BadRequest("Oops! Sorry, can't create a citizen without address.city."); 
            if (String.IsNullOrEmpty(citizen.Address.State)) return BadRequest("Oops! Sorry, can't create a citizen without address.state."); 
            if (String.IsNullOrEmpty(citizen.Address.PostalCode)) return BadRequest("Oops! Sorry, can't create a citizen without address.postalCode."); 
            if (String.IsNullOrEmpty(citizen.Address.Country)) return BadRequest("Oops! Sorry, can't create a citizen without address.country.");
            if (!String.IsNullOrEmpty(citizen.TenantId) && !citizen.TenantId.Equals(tenantId, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest("Oops! Sorry, can't create a citizen in a tenant other than your own.");
            if (String.IsNullOrEmpty(citizen.CitizenId)) citizen.CitizenId = Guid.NewGuid().ToString();
            citizen.TenantId = tenantId;
            #endregion

            try
            {   
                await _citizenRepository.AddCitizen(citizen, tenantId);

                //After creating citizen, call ProvisionAPI to provision default resource for citizen
                //Forward the auth token
                await _provisionService.ProvisionDefaultResource(citizen, Request.Headers["Authorization"]);

                var createdCitizen = await _citizenRepository.GetCitizen(citizen.CitizenId, tenantId);
                if (createdCitizen != null) return Created($"/Citizens/{citizen.CitizenId}", createdCitizen);
                //Return failure if the created citizen can't be retrieved successfully
                else return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something might have gone wrong.");
            }
            catch (DuplicateNameException)
            {
                return this.StatusCode(StatusCodes.Status409Conflict, "Oops! Sorry, a citizen with that citizenId already exists.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // DELETE /citizens/739ad8de-7b3b-45c1-a90c-697ef16317ce/
        [Authorize (Roles = "Global.Citizens.Write")]
        [HttpDelete("{citizenId}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult> DeleteV1(string citizenId)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var citizen = await _citizenRepository.GetCitizen(citizenId, tenantId);
                if (citizen == null) return NotFound("Oops! Sorry, can't find that citizen.");

                var deleteAccepted = await _citizenRepository.RemoveCitizen(citizenId, tenantId);

                //After deleting citizen, call ProvisionAPI to deprovision resources of citizen
                //Forward the auth token
                await _provisionService.DeprovisionAllResources(citizen, Request.Headers["Authorization"]);

                if (deleteAccepted) return this.StatusCode(StatusCodes.Status202Accepted);
                else return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something might have gone wrong.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Oops! Sorry, something bad happened.");
            }
        }

        // PUT /citizens/739ad8de-7b3b-45c1-a90c-697ef16317ce/
        [Authorize (Roles = "Global.Citizens.Write")]
        [HttpPut("{citizenId}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<Citizen>> PutV1(string citizenId, Citizen citizenUpdates)
        {
            //Get Azure AD Tenant Id of the caller from the auth token
            var tenantId = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");

            try
            {
                var oldCitizen = await _citizenRepository.GetCitizen(citizenId, tenantId);
                if (oldCitizen == null) return NotFound("Oops! Sorry, can't find that citizen.");

                #region Field Validation
                if (!String.IsNullOrEmpty(citizenUpdates.InternalId)) return BadRequest("Oops! Sorry, can't update internalId.");
                if (!String.IsNullOrEmpty(citizenUpdates.CitizenId)) return BadRequest("Oops! Sorry, can't update citizenId.");
                if (!String.IsNullOrEmpty(citizenUpdates.TenantId)) return BadRequest("Oops! Sorry, can't update tenantId.");
                if (!String.IsNullOrEmpty(citizenUpdates.GivenName)) oldCitizen.GivenName = citizenUpdates.GivenName;
                if (!String.IsNullOrEmpty(citizenUpdates.Surname)) oldCitizen.Surname = citizenUpdates.Surname;
                if (!String.IsNullOrEmpty(citizenUpdates.PhoneNumber)) oldCitizen.PhoneNumber = citizenUpdates.PhoneNumber;
                if (citizenUpdates.Address != null && !String.IsNullOrEmpty(citizenUpdates.Address.StreetAddress)) oldCitizen.Address.StreetAddress = citizenUpdates.Address.StreetAddress;
                if (citizenUpdates.Address != null && !String.IsNullOrEmpty(citizenUpdates.Address.City)) oldCitizen.Address.StreetAddress = citizenUpdates.Address.StreetAddress;
                if (citizenUpdates.Address != null && !String.IsNullOrEmpty(citizenUpdates.Address.State)) oldCitizen.Address.StreetAddress = citizenUpdates.Address.StreetAddress;
                if (citizenUpdates.Address != null && !String.IsNullOrEmpty(citizenUpdates.Address.PostalCode)) oldCitizen.Address.StreetAddress = citizenUpdates.Address.StreetAddress;
                if (citizenUpdates.Address != null && !String.IsNullOrEmpty(citizenUpdates.Address.Country)) oldCitizen.Address.StreetAddress = citizenUpdates.Address.StreetAddress;
                #endregion

                var updateAccepted = await _citizenRepository.ReplaceCitizen(citizenId, oldCitizen, tenantId);

                if (updateAccepted)
                {
                    //return updated citizen
                    var result = await _citizenRepository.GetCitizen(citizenId, tenantId);
                    return Ok(oldCitizen);
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
