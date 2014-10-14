/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;
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

            return new LogoutResult(/* SignOutMessage */ null, Request.GetOwinEnvironment(), this._options);
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