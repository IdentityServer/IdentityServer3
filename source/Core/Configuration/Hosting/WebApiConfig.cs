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

            if (options.LoggingOptions.EnableHttpLogging)
            {
                config.MessageHandlers.Add(new RequestResponseLogger());
            }

            return config;
        }

        private static void ConfigureRoutes(IdentityServerOptions options, HttpConfiguration config)
        {
            if (options.EnableWelcomePage)
            {
                config.Routes.MapHttpRoute(Constants.RouteNames.Welcome, Constants.RoutePaths.Welcome, new { controller = "Welcome", action = "Get" });
            }
        }

        private class HttpControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver _)
            {
                var httpControllerType = typeof (IHttpController);
                return typeof (WebApiConfig)
                    .Assembly
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && httpControllerType.IsAssignableFrom(t))
                    .ToList();
            }
        }

    }
}