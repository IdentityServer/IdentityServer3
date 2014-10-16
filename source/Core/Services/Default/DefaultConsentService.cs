/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
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