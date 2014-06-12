/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class LogoutController : ApiController
    {
        ILog logger;
        CoreSettings settings;

        public LogoutController(CoreSettings settings)
        {
            this.logger = LogProvider.GetCurrentClassLogger();
            this.settings = settings;
        }

        [Route("connect/logout", Name=Constants.RouteNames.LogoutPrompt)]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            logger.Info("[LogoutController.Logout] called");

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
