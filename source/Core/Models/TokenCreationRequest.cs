using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class TokenCreationRequest
    {
        public ClaimsPrincipal Subject { get; set; }
        public Client Client { get; set; }
        public IEnumerable<Scope> Scopes { get; set; }
        public ValidatedRequest ValidatedRequest { get; set; }

        public bool IncludeAllIdentityClaims { get; set; }
        public string AccessTokenToHash { get; set; }
        public string AuthorizationCodeToHash { get; set; }
    }
}