using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class EndpointSettings
    {
        public EndpointSettings()
        {
            Enabled = false;
        }

        public bool Enabled { get; set; }
    }
}