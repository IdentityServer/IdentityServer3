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
                    var factory = Factory.Create();
                    
                    var opts = new IdentityServerOptions
                    {
                        IssuerUri = "https://idsrv3.com",
                        SiteName = "Thinktecture IdentityServer v3 - preview 1",
                        SigningCertificate = Cert.Load(),
                        Factory = factory,
                        PublicHostName = "http://localhost:3333"
                    };

                    coreApp.UseIdentityServer(opts);
                });
        }
    }
}