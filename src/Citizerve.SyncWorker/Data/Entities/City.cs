using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Citizerve.SyncWorker.Data
{
    public class City
    {
        public int Id { get; set; }
        [Column("City")]
        public string CityName { get; set; }
        [Column("State")]
        public string StateName { get; set; }
        [Column("Country")]
        public string CountryName { get; set; }
        public string PostalCode { get; set; }
        public string AreaCode { get; set; }
    }
}
