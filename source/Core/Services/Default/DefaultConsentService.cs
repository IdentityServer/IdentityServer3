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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class DefaultConsentService : IConsentService
    {
        readonly IConsentStore _store;

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

            // TODO: validate that this is a correct statement
            if (!client.AllowRememberConsent)
            {
                return true;
            }

            if (scopes == null || !scopes.Any())
            {
                return false;
            }
            
            var consent = await _store.LoadAsync(user.GetSubjectId(), client.ClientId);
            if (consent != null && consent.Scopes != null)
            {
                var intersect = scopes.Intersect(consent.Scopes);
                return !(scopes.Count() == intersect.Count());
            }

            return true;
        }

        public async Task UpdateConsentAsync(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (user == null) throw new ArgumentNullException("user");

            if (client.AllowRememberConsent)
            {
                var subject = user.GetSubjectId();
                var clientId = client.ClientId;

                if (scopes != null && scopes.Any())
                {
                    var consent = new Consent
                    {
                        Subject = subject,
                        ClientId = clientId,
                        Scopes = scopes
                    };
                    await _store.UpdateAsync(consent);
                }
                else
                {
                    await _store.RevokeAsync(subject, clientId);
                }
            }
        }
    }
}
