using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Host.Config;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using Thinktecture.IdentityServer.WsFederation.Services;

[assembly: OwinStartup("LocalTest", typeof(Thinktecture.IdentityServer.Host.Startup_LocalTest))]

namespace Thinktecture.IdentityServer.Host
{
    public class Startup_LocalTest
    {
        public void Configuration(IAppBuilder app)
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());

            // uncomment to enable HSTS headers for the host
            // see: https://developer.mozilla.org/en-US/docs/Web/Security/HTTP_strict_transport_security
            //app.UseHsts();

            app.Map("/core", coreApp =>
                {
                    // allow cross origin calls
                    coreApp.UseCors(CorsOptions.AllowAll);

                    var factory = Factory.Create();
                    //factory.UserService = Registration.RegisterFactory<IUserService>(Thinktecture.IdentityServer.MembershipReboot.UserServiceFactory.Factory);
                    //factory.UserService = Registration.RegisterFactory<IUserService>(Thinktecture.IdentityServer.AspNetIdentity.UserServiceFactory.Factory);

                    var idsrvOptions = new IdentityServerOptions
                    {
                        IssuerUri = "https://idsrv3.com",
                        SiteName = "Thinktecture IdentityServer v3 - preview 1",
                        SigningCertificate = Cert.Load(),
                        CspReportEndpoint = EndpointSettings.Enabled,
                        AccessTokenValidationEndpoint = EndpointSettings.Enabled,
                        PublicHostName = "http://localhost:3333",
                        Factory = factory,
                        AdditionalIdentityProviderConfiguration = ConfigureAdditionalIdentityProviders,
                        ConfigurePlugins = ConfigurePlugins
                    };

                    coreApp.UseIdentityServer(idsrvOptions);
                });
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var wsFedOptions = new WsFederationPluginOptions
            {
                Options = options,
                Factory = new WsFederationServiceFactory
                {
                    UserService = options.Factory.UserService,
                    RelyingPartyService = Registration.RegisterFactory<IRelyingPartyService>(() => new InMemoryRelyingPartyService(RelyingParties.Get())),
                    WsFederationSettings = Registration.RegisterFactory<WsFederationSettings>(() => new WsFedSettings())
                }
            };
            
            pluginApp.UseWsFederationPlugin(wsFedOptions);
        }

        public static void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleAuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType
            };
            app.UseGoogleAuthentication(google);

            var fb = new FacebookAuthenticationOptions
            {
                AuthenticationType = "Facebook",
                SignInAsAuthenticationType = signInAsType,
                AppId = "676607329068058",
                AppSecret = "9d6ab75f921942e61fb43a9b1fc25c63"
            };
            app.UseFacebookAuthentication(fb);

            var twitter = new TwitterAuthenticationOptions
            {
                AuthenticationType = "Twitter",
                SignInAsAuthenticationType = signInAsType,
                ConsumerKey = "N8r8w7PIepwtZZwtH066kMlmq",
                ConsumerSecret = "df15L2x6kNI50E4PYcHS0ImBQlcGIt6huET8gQN41VFpUCwNjM"
            };
            app.UseTwitterAuthentication(twitter);
        }
    }
}