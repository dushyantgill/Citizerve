using Citizerve.CitizenAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Citizerve.CitizenAPI.Services
{
    public interface IProvisionService
    {
        Task ProvisionDefaultResource(Citizen citizen, string authorizeHeader);
        Task DeprovisionAllResources(Citizen citizen, string authorizeHeader);
    }
}
