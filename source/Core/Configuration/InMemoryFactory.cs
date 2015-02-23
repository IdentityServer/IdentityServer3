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

using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    /// <summary>
    /// Helper class to create <see cref="IdentityServerServiceFactory"/>.
    /// </summary>
    public static class InMemoryFactory
    {
        /// <summary>
        /// Convenience method to create an instance of <see cref="IdentityServerServiceFactory" /> and
        /// configure it to use in-memory implementations of the <see cref="IUserService"/>, <see cref="IClientStore"/>, 
        /// and <see cref="IScopeStore"/>.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="clients">The clients.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        public static IdentityServerServiceFactory Create(
            List<InMemoryUser> users = null,
            IEnumerable<Client> clients = null,
            IEnumerable<Scope> scopes = null)
        {
            var factory = new IdentityServerServiceFactory();
            
            if (users != null)
            {
                factory.Register(new Registration<List<InMemoryUser>>(users));
                factory.UserService = new Registration<IUserService, InMemoryUserService>();
            }

            if (clients != null)
            {
                factory.Register(new Registration<IEnumerable<Client>>(clients));
                factory.ClientStore = new Registration<IClientStore>(typeof(InMemoryClientStore));
            }

            if (scopes != null)
            {
                factory.Register(new Registration<IEnumerable<Scope>>(scopes));
                factory.ScopeStore = new Registration<IScopeStore>(typeof(InMemoryScopeStore));
            }

            return factory;
        }
    }
}