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
using IdentityServer3.Core.Extensions;
using System;
using System.Net.Http;
using System.Web.Http.Filters;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public SecurityHeadersAttribute()
        {
            EnableXfo = true;
            EnableCto = true;
            EnableCsp = true;
        }

        public bool EnableXfo { get; set; }
        public bool EnableCto { get; set; }
        public bool EnableCsp { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext != null &&
                actionExecutedContext.Response != null &&
                actionExecutedContext.Response.IsSuccessStatusCode &&
                (actionExecutedContext.Response.Content == null ||
                 "text/html".Equals(actionExecutedContext.Response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase))
            )
            {
                if (EnableCto)
                {
                    actionExecutedContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                if (EnableXfo)
                {
                    actionExecutedContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }

                if (EnableCsp)
                {
                    var ctx = actionExecutedContext.Request.GetOwinContext();
                    var scope = ctx.Environment.GetLifetimeScope();
                    var options = (IdentityServerOptions)scope.ResolveOptional(typeof(IdentityServerOptions));
                    if (options.CspOptions.Enabled)
                    {
                        // img-src as * due to client logos
                        var value = "default-src 'self'; script-src 'self' {0}; style-src 'self' 'unsafe-inline' {1}; img-src *;";

                        if (!String.IsNullOrWhiteSpace(options.CspOptions.FontSrc))
                        {
                            value += String.Format("font-src {0};", options.CspOptions.FontSrc);
                        }
                        if (!String.IsNullOrWhiteSpace(options.CspOptions.ConnectSrc))
                        {
                            value += String.Format("connect-src {0};", options.CspOptions.ConnectSrc);
                        }

                        value = String.Format(value, options.CspOptions.ScriptSrc, options.CspOptions.StyleSrc);
                        if (options.Endpoints.EnableCspReportEndpoint)
                        {
                            value += " report-uri " + ctx.GetCspReportUrl();
                        }
                        actionExecutedContext.Response.Headers.Add("Content-Security-Policy", value);
                    }
                }
            }
        }
    }
}