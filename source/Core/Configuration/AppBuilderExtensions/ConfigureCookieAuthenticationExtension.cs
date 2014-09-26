/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using System;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Owin
{
    static class UseCookieAuthenticationExtension
    {
        public static IAppBuilder ConfigureCookieAuthentication(this IAppBuilder app, CookieOptions options, IDataProtector dataProtector)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (dataProtector == null) throw new ArgumentNullException("dataProtector");

            if (options.Prefix != null && options.Prefix.Length > 0)
            {
                options.Prefix += ".";
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PrimaryAuthenticationType,
                CookieName = options.Prefix + Constants.PrimaryAuthenticationType,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration,
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.PrimaryAuthenticationType))
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.ExternalAuthenticationType,
                CookieName = options.Prefix + Constants.ExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = Constants.ExternalCookieTimeSpan,
                SlidingExpiration = false,
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.ExternalAuthenticationType))
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PartialSignInAuthenticationType,
                CookieName = options.Prefix + Constants.PartialSignInAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration,
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.PartialSignInAuthenticationType))
            });
            return app;
        }
    }
}