/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using System.Net.Http;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Views;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    internal class ErrorPageFilterAttribute : ExceptionFilterAttribute
    {
        public override async System.Threading.Tasks.Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, System.Threading.CancellationToken cancellationToken)
        {
            var env = actionExecutedContext.ActionContext.Request.GetOwinEnvironment();
            var scope = env.GetLifetimeScope();
            var options = (IdentityServerOptions)scope.ResolveOptional(typeof(IdentityServerOptions));
            var viewSvc = (IViewService)scope.ResolveOptional(typeof(IViewService));
            var errorModel = new ErrorViewModel
            {
                SiteName = options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl()
            };
            var errorResult = new ErrorActionResult(viewSvc, env, errorModel);
            actionExecutedContext.Response = await errorResult.GetResponseMessage();
        }
    }
}
