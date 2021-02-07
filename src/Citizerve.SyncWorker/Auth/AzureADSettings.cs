using System;
using System.Collections.Generic;
using System.Text;

namespace Citizerve.SyncWorker
{
    public class AzureADSettings
    {
        public string TokenUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Resource { get; set; }
    }
}
