/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default consent service
    /// </summary>
    public class DefaultConsentService : IConsentService
    {
        /// <summary>
        /// The consent store
        /// </summary>
        protected readonly IConsentStore _store;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConsentService"/> class.
        /// </summary>
        /// <param name="store">The consent store.</param>
        /// <exception cref="System.ArgumentNullException">store</exception>
        public DefaultConsentService(IConsentStore store)
        {
            if (store == null) throw new ArgumentNullException("store");

            this._store = store;
        }

        /// <summary>
        /// Checks if consent is required.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subject">The user.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns>Boolean if consent is required.</returns>
        public virtual async Task<bool> RequiresConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (subject == null) throw new ArgumentNullException("subject");

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

            // we always require consent for offline access if
            // the client has not disabled RequireConsent 
            if (scopes.Contains(Constants.StandardScopes.OfflineAccess))
            {
                return true;
            }
            
            var consent = await _store.LoadAsync(subject.GetSubjectId(), client.ClientId);
            if (consent != null && consent.Scopes != null)
            {
                var intersect = scopes.Intersect(consent.Scopes);
                return !(scopes.Count() == intersect.Count());
            }

            return true;
        }

        /// <summary>
        /// Updates the consent.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        public virtual async Task UpdateConsentAsync(Client client, ClaimsPrincipal subject, IEnumerable<string> scopes)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (subject == null) throw new ArgumentNullException("subject");

            if (client.AllowRememberConsent)
            {
                var subjectId = subject.GetSubjectId();
                var clientId = client.ClientId;

                if (scopes != null && scopes.Any())
                {
                    var consent = new Consent
                    {
                        Subject = subjectId,
                        ClientId = clientId,
                        Scopes = scopes
                    };
                    await _store.UpdateAsync(consent);
                }
                else
                {
                    await _store.RevokeAsync(subjectId, clientId);
                }
            }
        }
    }
}