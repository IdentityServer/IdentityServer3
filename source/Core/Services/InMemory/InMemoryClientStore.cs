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
    public class InMemoryClientStore : IClientStore
    {
        readonly IEnumerable<Client> _clients;

        public InMemoryClientStore(IEnumerable<Client> clients)
        {
            _clients = clients;
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var query =
                from client in _clients
                where client.ClientId == clientId && client.Enabled
                select client;
            
            return Task.FromResult(query.SingleOrDefault());
        }
    }
}