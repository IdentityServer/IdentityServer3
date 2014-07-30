using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Tests
{
    public class TestFactory
    {
        public static IdentityServerServiceFactory Create()
        {
            var scopes = new InMemoryScopeStore(TestScopes.Get());
            var clients = new InMemoryClientStore(TestClients.Get());
            
            var fact = new IdentityServerServiceFactory
            {
                ScopeStore = Registration.RegisterFactory<IScopeStore>(() => scopes),
                ClientStore = Registration.RegisterFactory<IClientStore>(() => clients)
            };

            return fact;
        }
    }
}
