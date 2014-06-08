using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class EndpointSettings
    {
        public EndpointSettings()
        {
            Enabled = true;
            AllowedOrigins = Enumerable.Empty<string>();
        }

        public bool Enabled { get; set; }
        public IEnumerable<string> AllowedOrigins { get; set; }
    }
}