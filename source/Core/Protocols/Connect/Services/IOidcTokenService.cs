using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Protocols.Connect;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public interface IOidcTokenService
    {
        Token CreateIdentityToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user);
        Token CreateIdentityToken(ValidatedTokenRequest request, ClaimsPrincipal user);

        Token CreateAccessToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user);
        Token CreateAccessToken(ValidatedTokenRequest request, ClaimsPrincipal user);

        string CreateJsonWebToken(Token token, OidcClient client, OidcConfiguration configuration);
    }
}