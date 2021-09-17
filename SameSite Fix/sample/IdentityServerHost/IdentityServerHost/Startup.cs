using IdentityServer3.Core.Configuration;
using IdentityServer3.Host.Config;
using IdentityServerHost.Config;
using Microsoft.Owin;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdentityServerHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var factory = new IdentityServerServiceFactory()
                    .UseInMemoryUsers(Users.Get())
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());

            var idsrvOptions = new IdentityServerOptions
            {
                RequireSsl = false,
                Factory = factory,
                SigningCertificate = Cert.Load(),
                AuthenticationOptions = new AuthenticationOptions
                {
                    IdentityProviders = ConfigureIdentityProviders,
                    CookieOptions = new IdentityServer3.Core.Configuration.CookieOptions
                    {
                        SuppressSameSiteNoneCookiesCallback = env =>
                        {
                            var context = new OwinContext(env);
                            var userAgent = context.Request.Headers["User-Agent"].ToString();

                            // Cover all iOS based browsers here. This includes:
                            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
                            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
                            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
                            // All of which are broken by SameSite=None, because they use the iOS 
                            // networking stack.
                            if (userAgent.Contains("CPU iPhone OS 12") ||
                                userAgent.Contains("iPad; CPU OS 12"))
                            {
                                return true;
                            }

                            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
                            // This includes:
                            // - Safari on Mac OS X.
                            // This does not include:
                            // - Chrome on Mac OS X
                            // Because they do not use the Mac OS networking stack.
                            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
                            {
                                return true;
                            }

                            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
                            // and none in this range require it.
                            // Note: this covers some pre-Chromium Edge versions, 
                            // but pre-Chromium Edge does not require SameSite=None.
                            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
                            {
                                return true;
                            }

                            return false;
                        }
                    }
                },
            };

            app.UseIdentityServer(idsrvOptions);
        }

        public static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var aad = new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = "is",
                Caption = "External OIDC",
                SignInAsAuthenticationType = signInAsType,

                Authority = "https://demo.identityserver.io",
                ClientId = "implicit",
                ResponseType = "id_token",
                RedirectUri = "https://localhost:44333/core/is",
            };
            app.UseSameSiteOpenIdConnectAuthentication(aad);
        }
    }
}