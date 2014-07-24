/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Hosting;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [SecurityHeaders]
    [NoCache]
    public class EndSessionController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;

        public EndSessionController(IdentityServerOptions options)
        {
            _options = options;
        }

        [Route(Constants.RoutePaths.Oidc.EndSession, Name = Constants.RouteNames.Oidc.EndSession)]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            Logger.Info("End session request");

            if (!_options.EndSessionEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            return Redirect(Url.Link(Constants.RouteNames.LogoutPrompt, null));
        }

        [Route(Constants.RoutePaths.Oidc.EndSessionCallback, Name = Constants.RouteNames.Oidc.EndSessionCallback)]
        [HttpGet]
        public IHttpActionResult LogoutCallback()
        {
            Logger.Info("End session callback requested");

            return Ok();
        }
    }
}