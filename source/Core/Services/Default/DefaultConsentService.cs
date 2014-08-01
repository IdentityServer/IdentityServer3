using System;
/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DefaultConsentService : IConsentService
    {
        IConsentStore _store;

        public DefaultConsentService(IConsentStore store)
        {
            if (store == null) throw new ArgumentNullException("store");

            this._store = store;
        }

        public async Task<bool> RequiresConsentAsync(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (user == null) throw new ArgumentNullException("user");

            if (!client.RequireConsent)
            {
                return false;
            }

            return await _store.RequiresConsentAsync(client.ClientId, user.GetSubjectId(), scopes);
        }

        public async Task UpdateConsentAsync(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (user == null) throw new ArgumentNullException("user");

            if (client.AllowRememberConsent)
            {
                await _store.UpdateConsentAsync(client.ClientId, user.GetSubjectId(), scopes);
            }
        }
    }
}