using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Citizerve.SyncWorker.Data
{
    public class Customer
    {
        public int Id { get; set; }
        [Column("Name")]
        public string CustomerName { get; set; }
        public string TenantId { get; set; }
    }
}
