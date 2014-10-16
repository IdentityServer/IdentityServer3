using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Owin;
using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Host;
using Thinktecture.IdentityServer.Host.Config;

[assembly: OwinStartup("LocalTest", typeof(Startup_LocalTest))]

namespace Thinktecture.IdentityServer.Host
{
    public class Startup_LocalTest
    {
        public void Configuration(IAppBuilder app)
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());
            //LogProvider.SetCurrentLogProvider(new TraceSourceLogProvider());

            // uncomment to enable HSTS headers for the host
            // see: https://developer.mozilla.org/en-US/docs/Web/Security/HTTP_strict_transport_security
            //app.UseHsts();

            app.Map("/core", coreApp =>
                {
                    var factory = Factory.Create();

                    var idsrvOptions = new IdentityServerOptions
                    {
                        IssuerUri = "https://idsrv3.com",
                        SiteName = "Thinktecture IdentityServer v3 - beta 2",
                        RequireSsl = false,
                        SigningCertificate = Cert.Load(),
                        CspOptions = new CspOptions {
                            ReportEndpoint = EndpointSettings.Enabled,
                        },
                        AccessTokenValidationEndpoint = EndpointSettings.Enabled,
                        Factory = factory,
                        AdditionalIdentityProviderConfiguration = ConfigureAdditionalIdentityProviders,
                        CorsPolicy = CorsPolicy.AllowAll,
                    };
                    coreApp.UseIdentityServer(idsrvOptions);
                });

            // only for showing the getting started index page
            app.UseStaticFiles();
        }

        public static void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "767400843187-8boio83mb57ruogr9af9ut09fkg56b27.apps.googleusercontent.com",
                ClientSecret = "5fWcBT0udKY7_b6E3gEiJlze"
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