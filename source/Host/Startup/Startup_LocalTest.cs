using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
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

            app.Map("/core", coreApp =>
                {
                    // allow cross origin calls
                    coreApp.UseCors(CorsOptions.AllowAll);

                    var factory = Factory.Create(
                        issuerUri: "https://idsrv3.com",
                        siteName: "Thinktecture IdentityServer v3 - preview 1",
                        publicHostAddress: "http://localhost:3333/core");

                    //factory.UserService = Registration.RegisterFactory<IUserService>(Thinktecture.IdentityServer.MembershipReboot.UserServiceFactory.Factory);
                    //factory.UserService = Registration.RegisterFactory<IUserService>(Thinktecture.IdentityServer.AspNetIdentity.UserServiceFactory.Factory);

                    var idsrvOptions = new IdentityServerOptions
                    {
                        Factory = factory,
                        AdditionalIdentityProviderConfiguration = ConfigureAdditionalIdentityProviders,
                        //ConfigurePlugins = ConfigurePlugins
                    };

                    coreApp.UseIdentityServer(idsrvOptions);

                });
        }

        //private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        //{
        //    var wsfedOptions = new WsFederationPluginOptions
        //    {
        //        // todo - also signoutcleanup is broken right now
        //        LoginPageUrl = "http://localhost:3333/core/login",
        //        LogoutPageUrl = "http://localhost:3333/core/connect/logout",

        //        Factory = new WsFederationServiceFactory
        //        {
        //            UserService = options.Factory.UserService,
        //            CoreSettings = options.Factory.CoreSettings,
        //            RelyingPartyService = () => new InMemoryRelyingPartyService(RelyingParties.Get()),
        //            WsFederationSettings = () => new WsFedSettings()
        //        },
        //    };

        //    pluginApp.UseWsFederationPlugin(wsfedOptions);
        //}

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