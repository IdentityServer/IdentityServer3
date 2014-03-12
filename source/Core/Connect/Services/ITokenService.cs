using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ITokenService
    {
        Token CreateIdentityToken(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, NameValueCollection request);
        Token CreateAccessToken(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, NameValueCollection request);

        string CreateJsonWebToken(Token token, SigningCredentials credentials);
    }
}