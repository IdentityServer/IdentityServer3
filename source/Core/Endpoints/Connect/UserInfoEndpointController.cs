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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.ResponseHandling;
using IdentityServer3.Core.Results;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
    /// <summary>
    /// OpenID Connect userinfo endpoint
    /// </summary>
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
                await RaiseFailureEventAsync(error);

                return NotFound();
            }

            var tokenUsageResult = await _tokenUsageValidator.ValidateAsync(request);
            if (tokenUsageResult.TokenFound == false)
            {
                var error = "No token found.";

                Logger.Error(error);
                await RaiseFailureEventAsync(error);
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }

            Logger.Info("Token found: " + tokenUsageResult.UsageType.ToString());

            var tokenResult = await _tokenValidator.ValidateAccessTokenAsync(
                tokenUsageResult.Token,
                Constants.StandardScopes.OpenId);

            if (tokenResult.IsError)
            {
                Logger.Error(tokenResult.Error);
                await RaiseFailureEventAsync(tokenResult.Error);
                return Error(tokenResult.Error);
            }

            // pass scopes/claims to profile service
            var subject = tokenResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject).Value;
            var scopes = tokenResult.Claims.Where(c => c.Type == Constants.ClaimTypes.Scope).Select(c => c.Value);

            var payload = await _generator.ProcessAsync(subject, scopes, tokenResult.Client);

            Logger.Info("End userinfo request");
            await RaiseSuccessEventAsync();

            return new UserInfoResult(payload);
        }

        IHttpActionResult Error(string error, string description = null)
        {
            return new ProtectedResourceErrorResult(error, description);
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.UserInfo);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            if (_options.EventsOptions.RaiseFailureEvents)
            {
                await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.UserInfo, error);
            }
        }
    }
}