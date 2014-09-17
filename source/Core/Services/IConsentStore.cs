/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IConsentStore
    {
        Task<bool> RequiresConsentAsync(string client, string subject, IEnumerable<string> scopes);
        Task UpdateConsentAsync(string client, string subject, IEnumerable<string> scopes);
    }
}