using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = LocalTestFactory.Create(
                    issuerUri: "https://idsrv3.com",
                    siteName: "Thinktecture IdentityServer v3 - preview 1 (SelfHost)",
                    publicHostAddress: "http://localhost:3333");

            var opts = new IdentityServerCoreOptions
            {
                Factory = factory,
            };

            appBuilder.UseIdentityServerCore(opts);
        }
    }
}