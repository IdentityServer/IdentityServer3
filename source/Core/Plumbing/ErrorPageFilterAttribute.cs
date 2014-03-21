using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Assets;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    public class ErrorPageFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var response = EmbeddedHtmlResult.GetResponseMessage(
                actionExecutedContext.Request,
                new LayoutModel
                {
                    Page = "error"
                });

            actionExecutedContext.Response = response;
        }
    }
}
