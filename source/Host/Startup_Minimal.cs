using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.TestServices;

namespace Thinktecture.IdentityServer.Host.Sample
{
    public class Startup_Minimal
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/core", coreApp =>
                {
                    var factory = TestOptionsFactory.Create(
                        issuerUri:         "https://idsrv3.com",
                        siteName:          "Thinktecture IdentityServer v3 - preview 1",
                        certificateName:   "CN=idsrv3test",
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