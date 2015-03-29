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

using System;
using System.Web.Http.Filters;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext != null &&
                actionExecutedContext.Response != null &&
                actionExecutedContext.Response.IsSuccessStatusCode)
            {
                var cc = new System.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoStore = true,
                    NoCache = true,
                    Private = true,
                    MaxAge = TimeSpan.Zero
                };
                actionExecutedContext.Response.Headers.CacheControl = cc;

                actionExecutedContext.Response.Headers.Add("Pragma", "no-cache");
            }
        }
    }
}
