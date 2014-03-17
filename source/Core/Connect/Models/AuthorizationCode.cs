using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class AuthorizationCode
    {
        public DateTime CreationTime { get; set; }

        public Client Client { get; set; }
        public ClaimsPrincipal User { get; set; }
    
        public bool IsOpenId { get; set; }
        public IEnumerable<Scope> RequestedScopes { get; set; }
        public Uri RedirectUri { get; set; }

        public AuthorizationCode()
        {
            CreationTime = DateTime.UtcNow;
        }
    }
}