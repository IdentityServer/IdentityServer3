/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using System;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using Thinktecture.IdentityServer.WsFederation.Hosting;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class PluginAppBuilderExtensions
    {
        public static IAppBuilder UseWsFederationPlugin(this IAppBuilder app, WsFederationPluginOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            options.Validate();

            options.IdentityServerOptions.ProtocolLogoutUrls.Add(options.LogoutUrl);

            app.Map(options.MapPath, wsfedApp =>
                {
                    wsfedApp.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationType = WsFederationPluginOptions.CookieName,
                        AuthenticationMode = AuthenticationMode.Passive,
                        CookieName = options.IdentityServerOptions.CookiePrefix + WsFederationPluginOptions.CookieName
                    });

                    wsfedApp.Use<AutofacContainerMiddleware>(AutofacConfig.Configure(options));
                    Microsoft.Owin.Infrastructure.SignatureConversions.AddConversions(app);
                    wsfedApp.UseWebApi(WebApiConfig.Configure());
                });

            return app;
        }
    }
}