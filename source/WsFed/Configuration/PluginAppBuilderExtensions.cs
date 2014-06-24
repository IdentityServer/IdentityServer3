/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using WsFederationConfiguration = Thinktecture.IdentityServer.WsFederation.Configuration;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class PluginAppBuilderExtensions
    {
        public static IAppBuilder UseWsFederationPlugin(this IAppBuilder app, WsFederationPluginOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            options.Validate();

            // todo
            var internalConfig = new InternalConfiguration();

            app.Map("/wsfed", wsfedApp =>
                {
                    wsfedApp.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationType = WsFederationPluginOptions.CookieName,
                        AuthenticationMode = AuthenticationMode.Passive
                    });

                    app.Use<AutofacContainerMiddleware>(WsFederationConfiguration.AutofacConfig.Configure(options, internalConfig));
                    Microsoft.Owin.Infrastructure.SignatureConversions.AddConversions(app);
                    app.UseWebApi(WsFederationConfiguration.WebApiConfig.Configure());
                });

            

            //options.Configuration.AddApiControllerAssembly(typeof(WsFederationController).Assembly);
            
            

            //options.Configuration.AddSignOutCallbackUrl("/wsfed/signout");

            return app;
        }
    }
}