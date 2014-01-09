using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    public class ValidatedTokenRequest
    {
        public Client Client { get; set; }
        public string GrantType { get; set; }
        public AuthorizationCode AuthorizationCode { get; set; }
    }
}
