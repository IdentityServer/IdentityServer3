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

using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    internal static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerOptions options)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();

            config.MessageHandlers.Insert(0, new KatanaDependencyResolver());
            config.Services.Add(typeof(IExceptionLogger), new LogProviderExceptionLogger());

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

            if (options.LoggingOptions.EnableHttpLogging)
            {
                config.MessageHandlers.Add(new RequestResponseLogger());
            }

            return config;
        }
    }
}