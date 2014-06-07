/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class LogoutController : ApiController
    {
        ILogger logger;
        CoreSettings settings;
        public LogoutController(ILogger logger, CoreSettings settings)
        {
            this.logger = logger;
            this.settings = settings;
        }

        [Route("connect/logout", Name=Constants.RouteNames.LogoutPrompt)]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            logger.Start("[LogoutController.Logout] called");

            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel
                {
                    Server = settings.SiteName,
                    Page = "logoutprompt",
                    PageModel = new
                    {
                        url = Url.Route(Constants.RouteNames.Logout, null)
                    }
                });
        }
    }
}
