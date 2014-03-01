using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidatedTokenRequest
    {
        public Client Client { get; set; }
        public string GrantType { get; set; }
        public AuthorizationCode AuthorizationCode { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string UserName { get; set; }

        public ClaimsPrincipal Subject { get; set; }
    }
}
