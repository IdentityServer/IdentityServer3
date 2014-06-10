/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IExternalClaimsFilter
    {
        IEnumerable<Claim> Filter(IdentityProvider provider, IEnumerable<Claim> claims);
    }
}