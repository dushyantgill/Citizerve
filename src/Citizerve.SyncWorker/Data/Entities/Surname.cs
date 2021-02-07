using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Citizerve.SyncWorker.Data
{
    public class Surname
    {
        public int Id { get; set; }
        [Column("Surname")]
        public string Name { get; set; }
    }
}
