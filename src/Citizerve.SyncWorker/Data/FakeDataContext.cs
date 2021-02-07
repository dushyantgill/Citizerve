using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citizerve.SyncWorker.Data
{
    class FakeDataContext : DbContext
    {
        // private DbContextOptions _dbContextOptions;
        public FakeDataContext(DbContextOptions<FakeDataContext> dbContextOptions) : base (dbContextOptions)
        {
            // this._dbContextOptions = dbContextOptions;
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<GivenName> GivenNames { get; set; }
        public DbSet<Surname> Surnames { get; set; }
        public DbSet<StreetName> StreetNames { get; set; }
        public DbSet<City> Cities { get; set; }
    }
}
