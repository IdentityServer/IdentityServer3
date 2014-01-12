
namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class TokenResponse
    {
        public string Jwt { get; set; }
        public string AccessTokenReference { get; set; }
        public int AccessTokenLifetime { get; set; }
    }
}
