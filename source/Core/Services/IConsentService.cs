/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Models
{
    public interface IConsentService
    {
        Task<bool> RequiresConsentAsync(Client client, ClaimsPrincipal user, IEnumerable<string> scopes);
        Task UpdateConsentAsync(Client client, ClaimsPrincipal user, IEnumerable<string> scopes);
    }
}