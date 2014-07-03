/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.StaticFiles;
using System;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Hosting;

namespace Owin
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseIdentityServerCore(this IAppBuilder app, IdentityServerCoreOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            var internalConfig = new InternalConfiguration();

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

            // thank you Microsoft for the clean syntax
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
            JwtSecurityTokenHandler.OutboundClaimTypeMap = ClaimMappings.None;

            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.PrimaryAuthenticationType });
            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.ExternalAuthenticationType, AuthenticationMode = AuthenticationMode.Passive });
            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.PartialSignInAuthenticationType, AuthenticationMode = AuthenticationMode.Passive });

            if (options.ConfigurePlugins != null)
            {
                options.ConfigurePlugins(app, options);
            }

            if (options.AdditionalIdentityProviderConfiguration != null)
            {
                options.AdditionalIdentityProviderConfiguration(app, Constants.ExternalAuthenticationType);
            }

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Assets")
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets/libs/fonts"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Assets.libs.bootstrap.fonts")
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            app.Use<AutofacContainerMiddleware>(AutofacConfig.Configure(options, internalConfig));
            Microsoft.Owin.Infrastructure.SignatureConversions.AddConversions(app);
            app.UseWebApi(WebApiConfig.Configure(options));

            return app;
        }
    }
}