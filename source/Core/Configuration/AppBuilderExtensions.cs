/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.StaticFiles;
using System;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Owin
{
    using Microsoft.Owin.Builder;
    using System.Collections.Generic;

    public static class AppBuilderExtensions
    {
        public static IdentityServerAppBuilder UseIdentityServerCore(this IAppBuilder app, IdentityServerCoreOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            // thank you Microsoft for the clean syntax
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
            JwtSecurityTokenHandler.OutboundClaimTypeMap = ClaimMappings.None;

            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.PrimaryAuthenticationType });
            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.ExternalAuthenticationType, AuthenticationMode = AuthenticationMode.Passive });
            app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = Constants.PartialSignInAuthenticationType, AuthenticationMode = AuthenticationMode.Passive });

            if (options.SocialIdentityProviderConfiguration != null)
            {
                options.SocialIdentityProviderConfiguration(app, Constants.ExternalAuthenticationType);
            }

            var pluginDependencies = new Dictionary<Type, Func<object>>();
            if (options.PluginConfiguration != null)
            {
                options.PluginConfiguration(app, pluginDependencies);
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

            app.Use<AutofacContainerMiddleware>(AutofacConfig.Configure(options, pluginDependencies));
            Microsoft.Owin.Infrastructure.SignatureConversions.AddConversions(app);
            app.UseWebApi(WebApiConfig.Configure(options));

            return new IdentityServerAppBuilder(app);
        }
    }
}