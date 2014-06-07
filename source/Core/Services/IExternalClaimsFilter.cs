/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Models
{
    public interface IExternalClaimsFilter
    {
        IEnumerable<Claim> Filter(IdentityProvider provider, IEnumerable<Claim> claims);
    }
}