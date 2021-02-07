using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citizerve.SyncWorker.Data
{
    class FakeDataRepository : IFakeDataRepository
    {
        private readonly FakeDataContext _context;

        public FakeDataRepository(FakeDataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<List<Customer>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<List<GivenName>> GetGivenNames()
        {
            return await _context.GivenNames.ToListAsync();
        }

        public async Task<List<Surname>> GetSurnames()
        {
            return await _context.Surnames.ToListAsync();
        }

        public async Task<List<StreetName>> GetStreetNames()
        {
            return await _context.StreetNames.ToListAsync();
        }

        public async Task<List<City>> GetCities()
        {
            return await _context.Cities.ToListAsync();
        }
    }
}
