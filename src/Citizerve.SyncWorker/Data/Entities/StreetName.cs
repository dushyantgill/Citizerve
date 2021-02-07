using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Citizerve.SyncWorker.Data
{
    public class StreetName
    {
        public int Id { get; set; }
        [Column("StreetName")]
        public string Name { get; set; }
    }
}
