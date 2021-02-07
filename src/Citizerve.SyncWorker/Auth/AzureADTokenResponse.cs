using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citizerve.SyncWorker
{
    public class AzureADTokenResponse
    {
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "ext_expires_in")]
        public string ExtExpiresIn { get; set; }

        [JsonProperty(PropertyName = "expires_on")]
        public string ExpiresOn { get; set; }

        [JsonProperty(PropertyName = "not_before")]
        public string NotBefore { get; set; }

        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }
}
