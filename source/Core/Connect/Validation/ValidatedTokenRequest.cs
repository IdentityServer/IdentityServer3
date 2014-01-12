using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidatedTokenRequest
    {
        public Client Client { get; set; }
        public string GrantType { get; set; }
        public AuthorizationCode AuthorizationCode { get; set; }
    }
}
