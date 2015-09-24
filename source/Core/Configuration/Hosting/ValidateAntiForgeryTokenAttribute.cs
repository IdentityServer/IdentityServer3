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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Results;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.ViewModels;
using Microsoft.Owin;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace IdentityServer3.Core.Configuration.Hosting
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    internal class ValidateAntiForgeryTokenAttribute : PreventUnsupportedRequestMediaTypesAttribute
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public ValidateAntiForgeryTokenAttribute()
            : base(allowFormUrlEncoded:true)
        {
        }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // first check for 415
            await base.OnAuthorizationAsync(actionContext, cancellationToken);

            if (actionContext.Response == null)
            {
                await ValidateTokens(actionContext);
            }
        }

        private static async Task ValidateTokens(HttpActionContext actionContext)
        {
            var env = actionContext.Request.GetOwinEnvironment();

            var success = actionContext.Request.Method == HttpMethod.Post &&
                          actionContext.Request.Content.IsFormData();
            if (success)
            {
                // ReadAsByteArrayAsync buffers the request body stream
                // we then put the buffered copy into the owin context
                // so we can read it in the IsTokenValid API without 
                // disturbing the actual stream in the HttpRequestMessage
                // that WebAPI uses it later for model binding. #lame
                var bytes = await actionContext.Request.Content.ReadAsByteArrayAsync();
                var ms = new MemoryStream(bytes);
                ms.Seek(0, SeekOrigin.Begin);
                var ctx = new OwinContext(env);
                ctx.Request.Body = ms;

                var antiForgeryToken = env.ResolveDependency<AntiForgeryToken>();
                success = await antiForgeryToken.IsTokenValid();
            }

            if (!success)
            {
                Logger.ErrorFormat("AntiForgery validation failed -- returning error page");

                var options = env.ResolveDependency<IdentityServerOptions>();
                var viewSvc = env.ResolveDependency<IViewService>();
                var localization = env.ResolveDependency<ILocalizationService>();

                var errorModel = new ErrorViewModel
                {
                    RequestId = env.GetRequestId(),
                    SiteName = options.SiteName,
                    SiteUrl = env.GetIdentityServerBaseUrl(),
                    ErrorMessage = localization.GetMessage(Resources.MessageIds.UnexpectedError),
                    CurrentUser = env.GetCurrentUserDisplayName(),
                    LogoutUrl = env.GetIdentityServerLogoutUrl(),
                };
                var errorResult = new ErrorActionResult(viewSvc, errorModel);
                actionContext.Response = await errorResult.GetResponseMessage();
            }
        }
    }
}
