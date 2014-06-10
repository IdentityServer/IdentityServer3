using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;

[assembly: OwinStartup("AzureWebSites", typeof(Thinktecture.IdentityServer.Host.Startup_AzureWebSites))]

namespace Thinktecture.IdentityServer.Host
{
    public class Startup_AzureWebSites
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/core", coreApp =>
                {
                    var factory = LocalTestFactory.Create(
                        issuerUri: "https://idsrv3.com",
                        siteName: "Thinktecture IdentityServer v3 - preview 1 (WAWS)",
                        publicHostAddress: "http://idsrv3.azurewebsites.net");

                    var options = new IdentityServerCoreOptions
                    {
                        Factory = factory,
                        AdditionalIdentityProviderConfiguration = ConfigureSocialIdentityProviders
                    };

                    coreApp.UseIdentityServerCore(options);
                });
        }

        public static void ConfigureSocialIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleAuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType
            };
            app.UseGoogleAuthentication(google);
        }
    }
}