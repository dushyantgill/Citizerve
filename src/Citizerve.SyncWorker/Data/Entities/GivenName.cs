using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Citizerve.SyncWorker.Data
{
    public class GivenName
    {
        public int Id { get; set; }
        [Column("GivenName")]
        public string Name { get; set; }
    }
}
