/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IConsentStore
    {
        Task<bool> RequiresConsentAsync(string client, string subject, IEnumerable<string> scopes);
        Task UpdateConsentAsync(string client, string subject, IEnumerable<string> scopes);
    }
}