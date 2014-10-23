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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryConsentStore : IConsentStore
    {
        private readonly List<Consent> _consents = new List<Consent>();

        public Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var query =
                from c in _consents
                where c.Subject == subject
                select c;
            return Task.FromResult<IEnumerable<Consent>>(query.ToArray());
        }

        public Task<Consent> LoadAsync(string subject, string client)
        {
            var query =
                from c in _consents
                where c.Subject == subject && c.ClientId == client
                select c;
            return Task.FromResult(query.SingleOrDefault());
        }

        public Task UpdateAsync(Consent consent)
        {
            // makes a snapshot as a DB would
            consent.Scopes = consent.Scopes.ToArray();

            var query =
                from c in _consents
                where c.Subject == consent.Subject && c.ClientId == consent.ClientId
                select c;
            var item = query.SingleOrDefault();
            if (item != null)
            {
                item.Scopes = consent.Scopes;
            }
            else
            {
                _consents.Add(consent);
            }
            return Task.FromResult(0);
        }

        public Task RevokeAsync(string subject, string client)
        {
            var query =
                from c in _consents
                where c.Subject == subject && c.ClientId == client
                select c;
            var item = query.SingleOrDefault();
            if (item != null)
            {
                _consents.Remove(item);
            }
            return Task.FromResult(0);
        }
    }
}