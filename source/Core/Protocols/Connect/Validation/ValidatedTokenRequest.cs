using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    public class ValidatedTokenRequest
    {
        public OidcClient Client { get; set; }
        public string GrantType { get; set; }
        public AuthorizationCode AuthorizationCode { get; set; }
    }
}
