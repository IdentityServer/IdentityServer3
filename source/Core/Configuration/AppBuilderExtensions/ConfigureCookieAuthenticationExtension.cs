/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Owin
{
    static class UseCookieAuthenticationExtension
    {
        public static IAppBuilder ConfigureCookieAuthentication(this IAppBuilder app, CookieOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            if (options.Prefix != null && options.Prefix.Length > 0)
            {
                options.Prefix += ".";
            }
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PrimaryAuthenticationType,
                CookieName = options.Prefix + Constants.PrimaryAuthenticationType,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.ExternalAuthenticationType,
                CookieName = options.Prefix + Constants.ExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = Constants.ExternalCookieTimeSpan,
                SlidingExpiration = false
            });
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PartialSignInAuthenticationType,
                CookieName = options.Prefix + Constants.PartialSignInAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration
            });
            return app;
        }
    }
}
