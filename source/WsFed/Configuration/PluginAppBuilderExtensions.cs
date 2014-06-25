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
using WsFederationConfiguration = Thinktecture.IdentityServer.WsFederation.Configuration;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class PluginAppBuilderExtensions
    {
        public static IAppBuilder UseWsFederationPlugin(this IAppBuilder app, WsFederationPluginOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            options.Validate();

            var internalConfig = new InternalConfiguration();

            // todo hacky!
            internalConfig.LoginPageUrl = options.LoginPageUrl;

            var settings = options.Factory.CoreSettings();
            if (settings.DataProtector == null)
            {
                var provider = app.GetDataProtectionProvider();
                if (provider == null)
                {
                    provider = new DpapiDataProtectionProvider("idsrv3");
                }

                internalConfig.DataProtector = new HostDataProtector(provider);
            }
            else
            {
                internalConfig.DataProtector = settings.DataProtector;
            }

            app.Map(options.MapPath, wsfedApp =>
                {
                    wsfedApp.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationType = WsFederationPluginOptions.CookieName,
                        AuthenticationMode = AuthenticationMode.Passive
                    });

                    wsfedApp.Use<AutofacContainerMiddleware>(WsFederationConfiguration.AutofacConfig.Configure(options, internalConfig));
                    Microsoft.Owin.Infrastructure.SignatureConversions.AddConversions(wsfedApp);
                    wsfedApp.UseWebApi(WsFederationConfiguration.WebApiConfig.Configure());
                });

            // todo
            //options.Configuration.AddSignOutCallbackUrl("/wsfed/signout");

            return app;
        }
    }
}