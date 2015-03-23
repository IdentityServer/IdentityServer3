﻿/*
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

using System;
using System.IdentityModel.Tokens;
using Autofac;
using Microsoft.Owin.Infrastructure;
using Owin;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration.AppBuilderExtensions
{
    /// <summary>
    /// Configuration extensions for identity server
    /// </summary>
    public static class UseIdentityServerExtension
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Startup");

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

            app.ConfigureRequestId();

            options.ProtocolLogoutUrls.Add(Constants.RoutePaths.Oidc.END_SESSION_CALLBACK);
            app.ConfigureDataProtectionProvider(options);

            app.ConfigureIdentityServerBaseUrl(options.PublicOrigin);
            app.ConfigureIdentityServerIssuer(options);

            var container = AutofacConfig.Configure(options);
            app.Use<AutofacContainerMiddleware>(container);

            app.UseCors();
            app.ConfigureCookieAuthentication(options.AuthenticationOptions.CookieOptions, options.DataProtector);

            if (options.PluginConfiguration != null)
            {
                options.PluginConfiguration(app, options);
            }

            if (options.AuthenticationOptions.IdentityProviders != null)
            {
                options.AuthenticationOptions.IdentityProviders(app, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            }

            app.UseEmbeddedFileServer();

            SignatureConversions.AddConversions(app);
            app.UseWebApi(WebApiConfig.Configure(options));

            using (var child = container.CreateScopeWithEmptyOwinContext())
            {
                var eventSvc = child.Resolve<IEventService>();
                DoStartupDiagnostics(options, eventSvc);
            }
            
            return app;
        }

        private static void DoStartupDiagnostics(IdentityServerOptions options, IEventService eventSvc)
        {
            var cert = options.SigningCertificate;
            
            if (cert == null)
            {
                Logger.Warn("No signing certificate configured.");
                eventSvc.RaiseNoCertificateConfiguredEvent();

                return;
            }
            if (!cert.HasPrivateKey || !cert.IsPrivateAccessAllowed())
            {
                Logger.Error("Signing certificate has not private key or private key is not accessible. Make sure the account running your application has access to the private key");
                eventSvc.RaiseCertificatePrivateKeyNotAccessibleEvent(cert);

                return;
            }
            if (cert.PublicKey.Key.KeySize < 2048)
            {
                Logger.Error("Signing certificate key length is less than 2048 bits.");
                eventSvc.RaiseCertificateKeyLengthTooShortEvent(cert);

                return;
            }

            var timeSpanToExpire = cert.NotAfter - DateTimeHelper.UtcNow;
            if (timeSpanToExpire < TimeSpan.FromDays(30))
            {
                Logger.Warn("The signing certificate will expire in the next 30 days: " + cert.NotAfter.ToString());
                eventSvc.RaiseCertificateExpiringSoonEvent(cert);

                return;
            }

            eventSvc.RaiseCertificateValidatedEvent(cert);
        }
    }
}