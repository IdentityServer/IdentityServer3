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

            options.Configuration.AddApiControllerAssembly(typeof(WsFederationController).Assembly);
            
            options.Configuration.AddTypeFactory(typeof(IRelyingPartyService), options.RelyingPartyService);
            
            options.Configuration.AddType(typeof(SignInValidator));
            options.Configuration.AddType(typeof(SignInResponseGenerator));
            options.Configuration.AddType(typeof(MetadataResponseGenerator));
            options.Configuration.AddType(typeof(CookieMiddlewareCookieService), typeof(ICookieService));

            options.Configuration.AddSignOutCallbackUrl("/wsfed/signout");

            return app;
        }
    }
}