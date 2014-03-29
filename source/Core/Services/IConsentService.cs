/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IConsentService
    {
        bool RequiresConsent(Client client, ClaimsPrincipal user, IEnumerable<string> scopes);
        void UpdateConsent(Client client, ClaimsPrincipal user, IEnumerable<string> scopes);
    }
}