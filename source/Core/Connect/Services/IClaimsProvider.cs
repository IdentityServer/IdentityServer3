using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IClaimsProvider
    {
        IEnumerable<Claim> GetIdentityTokenClaims(ClaimsPrincipal user, Client client, IEnumerable<string> scopes, ICoreSettings settings, bool includeAllIdentityClaims, IUserService profile);
        IEnumerable<Claim> GetAccessTokenClaims(ClaimsPrincipal user, Client client, IEnumerable<string> scopes, ICoreSettings settings, IUserService _profile);
    }
}
