/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using System;

namespace Owin
{
    internal static class UseCookieAuthenticationExtension
    {
        public static IAppBuilder ConfigureCookieAuthentication(this IAppBuilder app, CookieOptions options, IDataProtector dataProtector)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (dataProtector == null) throw new ArgumentNullException("dataProtector");

            if (options.Prefix.IsPresent())
            {
                options.Prefix += ".";
            }

            var primary = new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PrimaryAuthenticationType,
                CookieName = options.Prefix + Constants.PrimaryAuthenticationType,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration,
                CookieSecure = GetCookieSecure(options.SecureMode),
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.PrimaryAuthenticationType))
            };
            app.UseCookieAuthentication(primary);

            var external = new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.ExternalAuthenticationType,
                CookieName = options.Prefix + Constants.ExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = Constants.ExternalCookieTimeSpan,
                SlidingExpiration = false,
                CookieSecure = GetCookieSecure(options.SecureMode),
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.ExternalAuthenticationType))
            };
            app.UseCookieAuthentication(external);

            var partial = new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.PartialSignInAuthenticationType,
                CookieName = options.Prefix + Constants.PartialSignInAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ExpireTimeSpan = options.ExpireTimeSpan,
                SlidingExpiration = options.SlidingExpiration,
                CookieSecure = GetCookieSecure(options.SecureMode),
                TicketDataFormat = new TicketDataFormat(new DataProtectorAdapter(dataProtector, options.Prefix + Constants.PartialSignInAuthenticationType))
            };
            app.UseCookieAuthentication(partial);

            Action<string> setCookiePath = path =>
            {
                if (!String.IsNullOrWhiteSpace(path))
                {
                    primary.CookiePath = external.CookiePath = path;
                    partial.CookiePath = path;
                }
            };
            
            if (String.IsNullOrWhiteSpace(options.Path))
            {
                app.Use(async (ctx, next) =>
                {
                    // we only want this to run once, so assign to null once called 
                    // (and yes, it's possible that many callers hit this at same time, 
                    // but the set is idempotent)
                    if (setCookiePath != null)
                    {
                        setCookiePath(ctx.Request.PathBase.Value);
                        setCookiePath = null;
                    }
                    await next();
                });
            }
            else
            {
                setCookiePath(options.Path);
            }

            return app;
        }

        private static CookieSecureOption GetCookieSecure(CookieSecureMode cookieSecureMode)
        {
            switch (cookieSecureMode)
            {
                case CookieSecureMode.Always:
                    return CookieSecureOption.Always;
                case CookieSecureMode.SameAsRequest:
                    return CookieSecureOption.SameAsRequest;
                default:
                    throw new InvalidOperationException("Invalid CookieSecureMode");
            }
        }
    }
}