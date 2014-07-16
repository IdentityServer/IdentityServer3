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
        public static IAppBuilder UseIdentityServer(this IAppBuilder app, IdentityServerOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            
            var internalConfig = new InternalConfiguration();

            if (options.DataProtector == null)
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
                internalConfig.DataProtector = options.DataProtector;
            }

            // thank you Microsoft for the clean syntax
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
            JwtSecurityTokenHandler.OutboundClaimTypeMap = ClaimMappings.None;

            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.PrimaryAuthenticationType, CookieName = Constants.PrimaryAuthenticationType });
            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.ExternalAuthenticationType, CookieName = Constants.ExternalAuthenticationType, AuthenticationMode = AuthenticationMode.Passive });
            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.PartialSignInAuthenticationType, CookieName = Constants.PartialSignInAuthenticationType, AuthenticationMode = AuthenticationMode.Passive });

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
            app.UseWebApi(WebApiConfig.Configure());

            return app;
        }

        public static IAppBuilder UseHsts(this IAppBuilder app, TimeSpan duration)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (duration < TimeSpan.Zero) throw new ArgumentException("duration cannot be below zero");

            if (duration > TimeSpan.Zero)
            {
                int seconds = (int)duration.TotalSeconds;
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.IsSecure)
                    {
                        ctx.Response.Headers.Append("Strict-Transport-Security", "max-age:" + seconds);
                    }
                    await next();
                });
            }

            return app;
        }

        public static IAppBuilder UseHsts(this IAppBuilder app, int days = 30)
        {
            if (days < 0) throw new ArgumentException("days cannot be below zero");

            return app.UseHsts(TimeSpan.FromDays(days));
        }
    }
}