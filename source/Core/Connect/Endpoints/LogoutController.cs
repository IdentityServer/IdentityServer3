/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class LogoutController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly CoreSettings _settings;

        public LogoutController(CoreSettings settings)
        {
            _settings = settings;
        }

        [Route("connect/logout", Name=Constants.RouteNames.LogoutPrompt)]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            Logger.Info("End session request");

            if (!_settings.EndSessionEndpoint.Enabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

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