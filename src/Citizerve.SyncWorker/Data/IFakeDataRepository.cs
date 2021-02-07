using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Citizerve.SyncWorker.Data
{
    public interface IFakeDataRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();

        Task<List<Customer>> GetCustomers();
        Task<List<GivenName>> GetGivenNames();
        Task<List<Surname>> GetSurnames();
        Task<List<StreetName>> GetStreetNames();
        Task<List<City>> GetCities();
    }
}
