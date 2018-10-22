using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Filters;
using IdentityServer3.Core.Extensions;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class DiscoveryCacheControlAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            
            var ctx = actionExecutedContext.Request.GetOwinContext();
            var options = ctx.ResolveDependency<IdentityServerOptions>();
            SetCache(actionExecutedContext.Response, options.DiscoveryOptions.ClientCacheInterval);
        }

        public static void SetCache(HttpResponseMessage response, TimeSpan maxAge)
        {
            response.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = maxAge };
        }
    }
}
