using Citizerve.CitizenAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.CitizenAPI.Data
{
    public interface ICitizenRepository
    {
        Task<IEnumerable<Citizen>> GetCitizens(string tenantId);
        Task<Citizen> GetCitizen(string citizenId, string tenantId);
        Task<IEnumerable<Citizen>> SearchCitizens(string tenantId, string name, string postalCode, string city, string state, string country);
        Task AddCitizen(Citizen citizen, string tenantId);
        Task<bool> RemoveCitizen(string citizenId, string tenantId);
        Task<bool> ReplaceCitizen(string citizenId, Citizen citizen, string tenantId);
    }
}