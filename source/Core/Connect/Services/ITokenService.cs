using System.IdentityModel.Tokens;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ITokenService
    {
        Token CreateIdentityToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user);
        Token CreateAccessToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user);
        Token CreateAccessToken(ValidatedTokenRequest request);

        string CreateJsonWebToken(Token token, SigningCredentials credentials);
    }
}