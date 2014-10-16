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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryConsentStore : IConsentStore
    {
        private readonly ConcurrentBag<Consent> _consents = new ConcurrentBag<Consent>();
        
        public Task<bool> RequiresConsentAsync(string client, string subject, IEnumerable<string> scopes)
        {
            var orderedScopes = string.Join(" ", scopes.OrderBy(s => s).ToArray());

            var query = from c in _consents
                        where c.ClientId == client &&
                              c.Scopes == orderedScopes &&
                              c.Subject == subject
                        select c;

            var hit = query.FirstOrDefault();

            return Task.FromResult(hit == null);
        }

        public Task UpdateConsentAsync(string client, string subject, IEnumerable<string> scopes)
        {
            var orderedScopes = string.Join(" ", scopes.OrderBy(s => s).ToArray());
            
            _consents.Add(new Consent { ClientId = client, Subject = subject, Scopes = orderedScopes });

            return Task.FromResult(0);
        }
    }
}