using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext != null &&
                actionExecutedContext.Response != null &&
                actionExecutedContext.Response.IsSuccessStatusCode &&
                "text/html".Equals(actionExecutedContext.Response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase))
            {
                actionExecutedContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                actionExecutedContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");

                var cspReportUrl = actionExecutedContext.ActionContext.RequestContext.Url.Link(Constants.RouteNames.CspReport, null);
                actionExecutedContext.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src *; report-uri " + cspReportUrl);
            }
        }
    }
}
