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

using Autofac;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Owin
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
        /// <param name="options">The <see cref="IdentityServer3.Core.Configuration.IdentityServerOptions"/>.</param>
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
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            JwtSecurityTokenHandler.OutboundClaimTypeMap = new Dictionary<string, string>();

            if (options.RequireSsl)
            {
                app.Use<RequireSslMiddleware>();
            }

            if (options.LoggingOptions.EnableKatanaLogging)
            {
                app.SetLoggerFactory(new LibLogKatanaLoggerFactory());
            }

            app.ConfigureRequestId();

            options.ProtocolLogoutUrls.Add(Constants.RoutePaths.Oidc.EndSessionCallback);
            app.ConfigureDataProtectionProvider(options);

            app.ConfigureIdentityServerBaseUrl(options.PublicOrigin);
            app.ConfigureIdentityServerIssuer(options);

            var container = AutofacConfig.Configure(options);
            app.UseAutofacMiddleware(container);

            app.UseCors();
            app.ConfigureCookieAuthentication(options.AuthenticationOptions.CookieOptions, options.DataProtector);

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
            
            var httpConfig = WebApiConfig.Configure(options, container);
            app.UseAutofacWebApi(httpConfig);
            app.UseWebApi(httpConfig);

            using (var child = container.CreateScopeWithEmptyOwinContext())
            {
                var eventSvc = child.Resolve<IEventService>();
                // TODO -- perhaps use AsyncHelper instead?
                DoStartupDiagnosticsAsync(options, eventSvc).Wait();
            }
            
            return app;
        }

        private static async Task DoStartupDiagnosticsAsync(IdentityServerOptions options, IEventService eventSvc)
        {
            var cert = options.SigningCertificate;
            
            if (cert == null)
            {
                Logger.Warn("No signing certificate configured.");
                await eventSvc.RaiseNoCertificateConfiguredEventAsync();

                return;
            }
            if (!cert.HasPrivateKey || !cert.IsPrivateAccessAllowed())
            {
                Logger.Error("Signing certificate has not private key or private key is not accessible. Make sure the account running your application has access to the private key");
                await eventSvc.RaiseCertificatePrivateKeyNotAccessibleEventAsync(cert);

                return;
            }
            if (cert.PublicKey.Key.KeySize < 2048)
            {
                Logger.Error("Signing certificate key length is less than 2048 bits.");
                await eventSvc.RaiseCertificateKeyLengthTooShortEventAsync(cert);

                return;
            }

            var timeSpanToExpire = cert.NotAfter - DateTimeHelper.UtcNow;
            if (timeSpanToExpire < TimeSpan.FromDays(30))
            {
                Logger.Warn("The signing certificate will expire in the next 30 days: " + cert.NotAfter.ToString());
                await eventSvc.RaiseCertificateExpiringSoonEventAsync(cert);

                return;
            }

            await eventSvc.RaiseCertificateValidatedEventAsync(cert);
        }
    }
}