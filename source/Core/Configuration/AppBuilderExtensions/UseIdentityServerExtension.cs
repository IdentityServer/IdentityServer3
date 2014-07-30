/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin.Infrastructure;
using System;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Hosting;

namespace Owin
{
    public static class UseIdentityServerExtension
    {
        public static IAppBuilder UseIdentityServer(this IAppBuilder app, IdentityServerOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");

            options.Validate();

            // turn off weird claim mappings for JWTs
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
            JwtSecurityTokenHandler.OutboundClaimTypeMap = ClaimMappings.None;

            options.ProtocolLogoutUrls.Add(Constants.RoutePaths.Oidc.EndSessionCallback);
            app.ConfigureDataProtectionProvider(options);
            
            app.ConfigureIdentityServerBaseUrl(options.PublicHostName);
            app.UseCors(options.CorsPolicy);
            app.ConfigureCookieAuthentication(options.CookieOptions);
            
            if (options.PluginConfiguration != null)
            {
                options.PluginConfiguration(app, options);
            }

            if (options.AdditionalIdentityProviderConfiguration != null)
            {
                options.AdditionalIdentityProviderConfiguration(app, Constants.ExternalAuthenticationType);
            }

            app.UseEmbeddedFileServer();

            app.Use<AutofacContainerMiddleware>(AutofacConfig.Configure(options));
            SignatureConversions.AddConversions(app);
            app.UseWebApi(WebApiConfig.Configure());

            return app;
        }
    }
}