using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Host.Config
{
    public class Factory
    {
        public static IdentityServerServiceFactory Create(
                    string issuerUri, string siteName)
        {
            var settings = new Settings(issuerUri, siteName);

            return InMemoryFactory.Create(
                settings,
                Users.Get(),
                Clients.Get(),
                Scopes.Get());
        }
    }
}