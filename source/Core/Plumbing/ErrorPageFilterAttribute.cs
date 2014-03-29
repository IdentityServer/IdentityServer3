/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

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
