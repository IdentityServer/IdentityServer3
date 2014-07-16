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
            //app.UseHsts();

            app.Map("/core", coreApp =>
                {
                    var factory = Factory.Create(
                        issuerUri:         "https://idsrv3.com",
                        siteName:          "Thinktecture IdentityServer v3 - preview 1",
                        publicHostAddress: "http://localhost:3333");
                    
                    var opts = new IdentityServerOptions
                    {
                        Factory = factory,
                    };

                    coreApp.UseIdentityServer(opts);
                });
        }
    }
}