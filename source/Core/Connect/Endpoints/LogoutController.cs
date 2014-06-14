/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class LogoutController : ApiController
    {
        ILog _logger;
        CoreSettings _settings;

        public LogoutController(CoreSettings settings)
        {
            _logger = LogProvider.GetCurrentClassLogger();
            _settings = settings;
        }

        [Route("connect/logout", Name=Constants.RouteNames.LogoutPrompt)]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            _logger.Info("Logout called");

            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel
                {
                    Server = _settings.SiteName,
                    Page = "logoutprompt",
                    PageModel = new
                    {
                        url = Url.Route(Constants.RouteNames.Logout, null)
                    }
                });
        }
    }
}