/*
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OpenID Connect userinfo endpoint
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [RoutePrefix(Constants.RoutePaths.Oidc.UserInfo)]
    [NoCache]
    internal class UserInfoEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly UserInfoResponseGenerator _generator;
        private readonly TokenValidator _tokenValidator;
        private readonly BearerTokenUsageValidator _tokenUsageValidator;
        private readonly IdentityServerOptions _options;
        private readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoEndpointController"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="tokenValidator">The token validator.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="tokenUsageValidator">The token usage validator.</param>
        /// <param name="events">The event service</param>
        public UserInfoEndpointController(IdentityServerOptions options, TokenValidator tokenValidator, UserInfoResponseGenerator generator, BearerTokenUsageValidator tokenUsageValidator, IEventService events)
        {
            _tokenValidator = tokenValidator;
            _generator = generator;
            _options = options;
            _tokenUsageValidator = tokenUsageValidator;
            _events = events;
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>userinfo response</returns>
        [Route]
        [HttpGet, HttpPost]
        public async Task<IHttpActionResult> GetUserInfo(HttpRequestMessage request)
        {
            Logger.Info("Start userinfo request");

            if (!_options.Endpoints.EnableUserInfoEndpoint)
            {
                var error = "Endpoint is disabled. Aborting";
                Logger.Warn(error);
                RaiseFailureEvent(error);

                return NotFound();
            }

            var tokenUsageResult = await _tokenUsageValidator.ValidateAsync(request);
            if (tokenUsageResult.TokenFound == false)
            {
                var error = "No token found.";

                Logger.Error(error);
                RaiseFailureEvent(error);
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }

            Logger.Info("Token found: " + tokenUsageResult.UsageType.ToString());

            var tokenResult = await _tokenValidator.ValidateAccessTokenAsync(
                tokenUsageResult.Token,
                Constants.StandardScopes.OpenId);

            if (tokenResult.IsError)
            {
                Logger.Error(tokenResult.Error);
                RaiseFailureEvent(tokenResult.Error);
                return Error(tokenResult.Error);
            }

            // pass scopes/claims to profile service
            var subject = tokenResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject).Value;
            var scopes = tokenResult.Claims.Where(c => c.Type == Constants.ClaimTypes.Scope).Select(c => c.Value);

            var payload = await _generator.ProcessAsync(subject, scopes);

            Logger.Info("End userinfo request");
            RaiseSuccessEvent();

            return new UserInfoResult(payload);
        }

        IHttpActionResult Error(string error, string description = null)
        {
            return new ProtectedResourceErrorResult(error, description);
        }

        private void RaiseSuccessEvent()
        {
            _events.RaiseSuccessfulEndpointEvent(EventConstants.EndpointNames.UserInfo);
        }

        private void RaiseFailureEvent(string error)
        {
            if (_options.EventsOptions.RaiseFailureEvents)
            {
                _events.RaiseFailureEndpointEvent(EventConstants.EndpointNames.UserInfo, error);
            }
        }
    }
}