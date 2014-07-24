/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Web.Http.Filters;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext != null &&
                actionExecutedContext.Response != null &&
                actionExecutedContext.Response.IsSuccessStatusCode)
            {
                var cc = new System.Net.Http.Headers.CacheControlHeaderValue();
                cc.NoStore = true;
                cc.NoCache = true;
                cc.Private = true;
                cc.MaxAge = TimeSpan.Zero;
                actionExecutedContext.Response.Headers.CacheControl = cc;

                actionExecutedContext.Response.Headers.Add("Pragma", "no-cache");
            }
        }
    }
}
