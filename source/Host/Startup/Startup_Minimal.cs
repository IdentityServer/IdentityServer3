using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;

[assembly: OwinStartup("Minimal", typeof(Thinktecture.IdentityServer.Host.Startup_LocalTest))]

namespace Thinktecture.IdentityServer.Host.Sample
{
    public class Startup_Minimal
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/core", coreApp =>
                {
                    var factory = LocalTestFactory.Create(
                        issuerUri:         "https://idsrv3.com",
                        siteName:          "Thinktecture IdentityServer v3 - preview 1",
                        publicHostAddress: "http://localhost:3333");
                    
                    var opts = new IdentityServerCoreOptions
                    {
                        Factory = factory,
                    };

                    coreApp.UseIdentityServerCore(opts);
                });
        }
    }
}