using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IClaimsProvider
    {
        IEnumerable<Claim> GetIdentityTokenClaims(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ICoreSettings settings, bool includeAllIdentityClaims, IUserService profile, NameValueCollection request);
        IEnumerable<Claim> GetAccessTokenClaims(ClaimsPrincipal user, Client client, IEnumerable<Scope> scopes, ICoreSettings settings, IUserService _profile, NameValueCollection request);
    }
}
