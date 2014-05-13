/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.WsFed.Configuration;
using Thinktecture.IdentityServer.WsFed.ResponseHandling;
using Thinktecture.IdentityServer.WsFed.Services;
using Thinktecture.IdentityServer.WsFed.Validation;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class PluginAppBuilderExtensions
    {
        public static IAppBuilder UseWsFederationPlugin(this IAppBuilder app, WsFederationPluginOptions options)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = WsFederationConfiguration.WsFedCookieAuthenticationType,
                    AuthenticationMode = AuthenticationMode.Passive
                });

            options.Dependencies.Add(typeof(IRelyingPartyService), options.RelyingPartyService);

            options.Dependencies.Add(typeof(SignInValidator), null);
            options.Dependencies.Add(typeof(SignInResponseGenerator), null);
            options.Dependencies.Add(typeof(MetadataResponseGenerator), null);
            
            return app;
        }
    }
}