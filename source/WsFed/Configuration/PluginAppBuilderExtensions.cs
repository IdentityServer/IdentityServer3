/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.WsFed;
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
                    AuthenticationType = WsFederationPluginOptions.WsFedCookieAuthenticationType,
                    AuthenticationMode = AuthenticationMode.Passive
                });

            if (options.RelyingPartyService == null)
            {
                throw new ArgumentNullException("RelyingPartyService");
            }

            options.Dependencies.AddApiControllerAssembly(typeof(WsFederationController).Assembly);
            
            options.Dependencies.AddTypeFactory(typeof(IRelyingPartyService), options.RelyingPartyService);
            
            options.Dependencies.AddType(typeof(SignInValidator));
            options.Dependencies.AddType(typeof(SignInResponseGenerator));
            options.Dependencies.AddType(typeof(MetadataResponseGenerator));
            options.Dependencies.AddType(typeof(CookieMiddlewareCookieService), typeof(ICookieService));

            return app;
        }
    }
}