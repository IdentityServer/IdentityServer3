using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class InMemoryFactory
    {
        public static IdentityServerServiceFactory Create(
            CoreSettings settings,
            IEnumerable<InMemoryUser> users,
            IEnumerable<Client> clients,
            IEnumerable<Scope> scopes)
        {
            var userService = new InMemoryUserService(users);
            var scopeService = new InMemoryScopeService(scopes);
            var clientService = new InMemoryClientService(clients);

            var factory = new IdentityServerServiceFactory
            {
                UserService = Registration.RegisterFactory<IUserService>(() => userService),
                CoreSettings = Registration.RegisterFactory<CoreSettings>(() => settings),
                ScopeService = Registration.RegisterFactory<IScopeService>(() => scopeService),
                ClientService = Registration.RegisterFactory<IClientService>(() => clientService)
            };

            return factory;
        }
    }
}