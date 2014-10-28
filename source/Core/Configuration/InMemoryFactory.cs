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
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class InMemoryFactory
    {
        public static IdentityServerServiceFactory Create(
            IEnumerable<InMemoryUser> users = null,
            IEnumerable<Client> clients = null,
            IEnumerable<Scope> scopes = null)
        {
            var factory = new IdentityServerServiceFactory();
            
            if (users != null)
            {
                var userService = new InMemoryUserService(users);
                factory.UserService = Registration.RegisterFactory<IUserService>(() => userService);
            }

            if (clients != null)
            {
                var clientStore = new InMemoryClientStore(clients);
                factory.ClientStore = Registration.RegisterFactory<IClientStore>(() => clientStore);
            }

            if (scopes != null)
            {
                var scopeStore = new InMemoryScopeStore(scopes);
                factory.ScopeStore = Registration.RegisterFactory<IScopeStore>(() => scopeStore);
            }

            return factory;
        }
    }
}