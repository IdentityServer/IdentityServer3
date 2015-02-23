﻿/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using System.ComponentModel;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Validation;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OpenID Connect end session endpoint
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SecurityHeaders]
    [NoCache]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    public class EndSessionController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly EndSessionRequestValidator _validator;
        private readonly EndSessionResponseGenerator _generator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndSessionController"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="generator">The generator.</param>
        public EndSessionController(IdentityServerOptions options, EndSessionRequestValidator validator, EndSessionResponseGenerator generator)
        {
            _options = options;
            _validator = validator;
            _generator = generator;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns></returns>
        [Route(Constants.RoutePaths.Oidc.EndSession, Name = Constants.RouteNames.Oidc.EndSession)]
        [HttpGet]
        public async Task<IHttpActionResult> Logout()
        {
            Logger.Info("Start end session request");

            if (!_options.Endpoints.EnableEndSessionEndpoint)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var result = await _validator.ValidateAsync(Request.RequestUri.ParseQueryString(), User as ClaimsPrincipal);
            if (result.IsError)
            {
                // if anything went wrong, ignore the params the RP sent
                return new LogoutResult(null, Request.GetOwinEnvironment(), this._options);
            }
        
            var message = _generator.CreateSignoutMessage(_validator.ValidatedRequest);

            Logger.Info("End end session request");
            return new LogoutResult(message, Request.GetOwinEnvironment(), this._options);
        }

        /// <summary>
        /// Logout callback
        /// </summary>
        /// <returns></returns>
        [Route(Constants.RoutePaths.Oidc.EndSessionCallback, Name = Constants.RouteNames.Oidc.EndSessionCallback)]
        [HttpGet]
        public IHttpActionResult LogoutCallback()
        {
            Logger.Info("End session callback requested");

            return Ok();
        }
    }
}