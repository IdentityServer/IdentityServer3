using System;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class AuthorizationCode
    {
        public string ClientId { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsOpenId { get; set; }

        public string RequestedScopes { get; set; }
        public Uri RedirectUri { get; set; }

        public Token IdentityToken { get; set; }
        public Token AccessToken { get; set; }

        public AuthorizationCode()
        {
            CreationTime = DateTime.UtcNow;
        }
    }
}
