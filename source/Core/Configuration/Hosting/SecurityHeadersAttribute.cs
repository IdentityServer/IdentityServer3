/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using System;
using System.Net.Http;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
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
                        var value = "default-src 'self'; script-src 'self' {0}; style-src 'self' 'unsafe-inline' {1}; img-src *;";
                        value = String.Format(value, options.CspOptions.ScriptSrc, options.CspOptions.StyleSrc);
                        if (options.CspOptions.ReportEndpoint.IsEnabled)
                        {
                            var cspReportUrl = actionExecutedContext.ActionContext.RequestContext.Url.Link(Constants.RouteNames.CspReport, null);
                            value += " report-uri " + cspReportUrl;
                        }
                        actionExecutedContext.Response.Headers.Add("Content-Security-Policy", value);
                    }
                }
            }
        }
    }
}