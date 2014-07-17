using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;

[assembly: OwinStartup("Minimal", typeof(Thinktecture.IdentityServer.Host.Startup_Minimal))]

namespace Thinktecture.IdentityServer.Host
{
    public class Startup_Minimal
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/core", coreApp =>
                {
                    var settings = new Settings(
                        issuerUri: "https://idsrv3.com",
                        siteName: "Thinktecture IdentityServer v3 - preview 1");

                    var factory = InMemoryFactory.Create(
                        settings,
                        Users.Get(),
                        Clients.Get(),
                        Scopes.Get());
                    
                    var opts = new IdentityServerOptions
                    {
                        Factory = factory,
                        PublicHostName = "http://localhost:3333"
                    };

                    coreApp.UseIdentityServer(opts);
                });
        }
    }
}