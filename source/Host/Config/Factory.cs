using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Host.Config
{
    public class Factory
    {
        public static IdentityServerServiceFactory Create()
        {
            return InMemoryFactory.Create(
                Users.Get(),
                Clients.Get(),
                Scopes.Get());
        }
    }
}