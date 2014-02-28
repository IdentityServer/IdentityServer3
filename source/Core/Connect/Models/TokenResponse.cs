
namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class TokenResponse
    {
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public int AccessTokenLifetime { get; set; }
    }
}
