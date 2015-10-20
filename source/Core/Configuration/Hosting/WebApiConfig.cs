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
using Autofac.Integration.WebApi;
using Autofac.Util;
using IdentityServer3.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerOptions options, ILifetimeScope container)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            config.Services.Add(typeof(IExceptionLogger), new LogProviderExceptionLogger());
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new HttpControllerTypeResolver());
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;

            if (options.LoggingOptions.EnableWebApiDiagnostics)
            {
                var liblog = new TraceSource("LibLog");
                liblog.Switch.Level = SourceLevels.All;
                liblog.Listeners.Add(new LibLogTraceListener());

                var diag = config.EnableSystemDiagnosticsTracing();
                diag.IsVerbose = options.LoggingOptions.WebApiDiagnosticsIsVerbose;
                diag.TraceSource = liblog;
            }

            ConfigureRoutes(options, config);

            return config;
        }

        private static void ConfigureRoutes(IdentityServerOptions options, HttpConfiguration config)
        {
            if (options.EnableWelcomePage)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Welcome, 
                    Constants.RoutePaths.Welcome, 
                    new { controller = "Welcome", action = "Get" });
            }

            if (options.Endpoints.EnableAccessTokenValidationEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.AccessTokenValidation,
                    Constants.RoutePaths.Oidc.AccessTokenValidation,
                    new { controller = "AccessTokenValidation" });
            }

            if (options.Endpoints.EnableIntrospectionEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Introspection,
                    Constants.RoutePaths.Oidc.Introspection,
                    new { controller = "IntrospectionEndpoint" });
            }

            if (options.Endpoints.EnableAuthorizeEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Authorize,
                    Constants.RoutePaths.Oidc.Authorize,
                    new { controller = "AuthorizeEndpoint", action = "Get" });
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Consent,
                    Constants.RoutePaths.Oidc.Consent,
                    new { controller = "AuthorizeEndpoint", action = "PostConsent" });
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.SwitchUser,
                    Constants.RoutePaths.Oidc.SwitchUser,
                    new { controller = "AuthorizeEndpoint", action = "LoginAsDifferentUser" });
            }

            if (options.Endpoints.EnableCheckSessionEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.CheckSession,
                    Constants.RoutePaths.Oidc.CheckSession,
                    new { controller = "CheckSessionEndpoint" });
            }

            if (options.Endpoints.EnableClientPermissionsEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.ClientPermissions,
                    Constants.RoutePaths.ClientPermissions,
                    new { controller = "ClientPermissions" });
            }

            if (options.Endpoints.EnableCspReportEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.CspReport,
                    Constants.RoutePaths.CspReport,
                    new { controller = "CspReport" });
            }

            if (options.Endpoints.EnableDiscoveryEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.DiscoveryConfiguration,
                    Constants.RoutePaths.Oidc.DiscoveryConfiguration,
                    new { controller = "DiscoveryEndpoint", action = "GetConfiguration" });
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.DiscoveryWebKeys,
                    Constants.RoutePaths.Oidc.DiscoveryWebKeys,
                    new { controller = "DiscoveryEndpoint", action= "GetKeyData" });
            }

            if (options.Endpoints.EnableEndSessionEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.EndSession,
                    Constants.RoutePaths.Oidc.EndSession,
                    new { controller = "EndSession", action = "Logout" });
            }
            
            // this one is always enabled/allowed (for use by our logout page)
            config.Routes.MapHttpRoute(
                Constants.RouteNames.Oidc.EndSessionCallback,
                Constants.RoutePaths.Oidc.EndSessionCallback,
                new { controller = "EndSession", action = "LogoutCallback" });

            if (options.Endpoints.EnableIdentityTokenValidationEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.IdentityTokenValidation,
                    Constants.RoutePaths.Oidc.IdentityTokenValidation,
                    new { controller = "IdentityTokenValidation" });
            }

            if (options.Endpoints.EnableTokenEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Token,
                    Constants.RoutePaths.Oidc.Token,
                    new { controller = "TokenEndpoint", action= "Post" });
            }

            if (options.Endpoints.EnableTokenRevocationEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.Revocation,
                    Constants.RoutePaths.Oidc.Revocation,
                    new { controller = "RevocationEndpoint", action = "Post" });
            }

            if (options.Endpoints.EnableUserInfoEndpoint)
            {
                config.Routes.MapHttpRoute(
                    Constants.RouteNames.Oidc.UserInfo,
                    Constants.RoutePaths.Oidc.UserInfo,
                    new { controller = "UserInfoEndpoint" });
            }
        }

        private class HttpControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver _)
            {
                var httpControllerType = typeof (IHttpController);
                return typeof (WebApiConfig)
                    .Assembly
                    .GetLoadableTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && httpControllerType.IsAssignableFrom(t))
                    .ToList();
            }
        }

    }
}
