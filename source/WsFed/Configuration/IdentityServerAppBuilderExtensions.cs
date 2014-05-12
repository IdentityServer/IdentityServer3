/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Thinktecture.IdentityServer.WsFed.Configuration;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class IdentityServerAppBuilderExtensions
    {
        public static IdentityServerAppBuilder WithWsFed(this IdentityServerAppBuilder app)
        {
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //    {
            //        AuthenticationType = WsFederationConfiguration.WsFedCookieAuthenticationType,
            //        AuthenticationMode = AuthenticationMode.Passive
            //    });

            return app;
        }
    }
}