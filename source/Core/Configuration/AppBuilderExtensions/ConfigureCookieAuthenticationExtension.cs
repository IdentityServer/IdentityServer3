using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Owin
{
    static class UseCookieAuthenticationExtension
    {
        public static IAppBuilder ConfigureCookieAuthentication(this IAppBuilder app, IdentityServerOptions options)
        {
            if (options.CookiePrefix != null && options.CookiePrefix.Length > 0)
            {
                options.CookiePrefix += ".";
            }
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PrimaryAuthenticationType,
                CookieName = options.CookiePrefix + Constants.PrimaryAuthenticationType,
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.ExternalAuthenticationType,
                CookieName = options.CookiePrefix + Constants.ExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PartialSignInAuthenticationType,
                CookieName = options.CookiePrefix + Constants.PartialSignInAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive
            });
            return app;
        }
    }
}
