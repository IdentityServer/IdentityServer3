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

using System.Net.Http;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    internal class ErrorPageFilterAttribute : ExceptionFilterAttribute
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        public override async System.Threading.Tasks.Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, System.Threading.CancellationToken cancellationToken)
        {
            Logger.ErrorException("Exception accessing: " + actionExecutedContext.Request.RequestUri.AbsolutePath, actionExecutedContext.Exception);

            var env = actionExecutedContext.ActionContext.Request.GetOwinEnvironment();
            var options = env.ResolveDependency<IdentityServerOptions>();
            var viewSvc = env.ResolveDependency<IViewService>();
            var errorModel = new ErrorViewModel
            {
                SiteName = options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                ErrorMessage = Resources.Messages.UnexpectedError,
            };
            var errorResult = new ErrorActionResult(viewSvc, env, errorModel);
            actionExecutedContext.Response = await errorResult.GetResponseMessage();
        }
    }
}