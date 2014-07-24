/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
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
                var clientService = new InMemoryClientService(clients);
                factory.ClientService = Registration.RegisterFactory<IClientService>(() => clientService);
            }

            if (scopes != null)
            {
                var scopeService = new InMemoryScopeService(scopes);
                factory.ScopeService = Registration.RegisterFactory<IScopeService>(() => scopeService);
            }

            return factory;
        }
    }
}