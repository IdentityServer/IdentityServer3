/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IClaimsProvider
    {
        Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ICoreSettings settings, bool includeAllIdentityClaims, IUserService profile, NameValueCollection request);
        Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal user, Client client, IEnumerable<Scope> scopes, ICoreSettings settings, IUserService _profile, NameValueCollection request);
    }
}
