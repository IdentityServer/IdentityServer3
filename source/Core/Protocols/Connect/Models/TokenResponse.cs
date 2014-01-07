using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Models
{
    public class TokenResponse
    {
        public string Jwt { get; set; }
        public string AccessTokenReference { get; set; }
        public int AccessTokenLifetime { get; set; }
    }
}
