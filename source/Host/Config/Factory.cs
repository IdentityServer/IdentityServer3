using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Host.Config
{
    public class Factory
    {
        public static IdentityServerServiceFactory Create()
        {
            var factory = InMemoryFactory.Create(
                Users.Get(),
                Clients.Get(),
                Scopes.Get());

            return factory;
        }
    }
}