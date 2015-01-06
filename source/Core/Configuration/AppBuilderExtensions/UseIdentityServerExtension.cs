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

using Microsoft.Owin.Infrastructure;
using System;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;

namespace Owin
{
    /// <summary>
    /// Configuration extensions for identity server
    /// </summary>
    public static class UseIdentityServerExtension
    {
        /// <summary>
        /// Extension method to configure IdentityServer in the hosting application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="options">The <see cref="Thinktecture.IdentityServer.Core.Configuration.IdentityServerOptions"/>.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// app
        /// or
        /// options
        /// </exception>
        public static IAppBuilder UseIdentityServer(this IAppBuilder app, IdentityServerOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");

            options.Validate();

            // turn off weird claim mappings for JWTs
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
            JwtSecurityTokenHandler.OutboundClaimTypeMap = ClaimMappings.None;

            if (options.RequireSsl)
            {
                app.Use<RequireSslMiddleware>();
            }

            options.ProtocolLogoutUrls.Add(Constants.RoutePaths.Oidc.EndSessionCallback);
            app.ConfigureDataProtectionProvider(options);

            app.ConfigureIdentityServerBaseUrl(options.PublicHostName);
            app.ConfigureIdentityServerIssuer(options);

            app.UseCors(options.CorsPolicy);
            app.ConfigureCookieAuthentication(options.AuthenticationOptions.CookieOptions, options.DataProtector);

            app.Use<AutofacContainerMiddleware>(AutofacConfig.Configure(options));
            
            if (options.PluginConfiguration != null)
            {
                options.PluginConfiguration(app, options);
            }

            if (options.AuthenticationOptions.IdentityProviders != null)
            {
                options.AuthenticationOptions.IdentityProviders(app, Constants.ExternalAuthenticationType);
            }

            app.UseEmbeddedFileServer();

            SignatureConversions.AddConversions(app);
            app.UseWebApi(WebApiConfig.Configure(options));

            return app;
        }
    }
}