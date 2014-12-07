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

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OpenID Connect userinfo endpoint
    /// </summary>
    [RoutePrefix(Constants.RoutePaths.Oidc.UserInfo)]
    [NoCache]
    public class UserInfoEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly UserInfoResponseGenerator _generator;
        private readonly TokenValidator _tokenValidator;
        private readonly BearerTokenUsageValidator _tokenUsageValidator;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoEndpointController"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="tokenValidator">The token validator.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="tokenUsageValidator">The token usage validator.</param>
        public UserInfoEndpointController(IdentityServerOptions options, TokenValidator tokenValidator, UserInfoResponseGenerator generator, BearerTokenUsageValidator tokenUsageValidator)
        {
            _tokenValidator = tokenValidator;
            _generator = generator;
            _options = options;
            _tokenUsageValidator = tokenUsageValidator;
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

            if (!_options.Endpoints.UserInfoEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var tokenUsageResult = await _tokenUsageValidator.ValidateAsync(request);
            if (tokenUsageResult.TokenFound == false)
            {
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }

            var tokenResult = await _tokenValidator.ValidateAccessTokenAsync(
                tokenUsageResult.Token, 
                Constants.StandardScopes.OpenId);

            if (tokenResult.IsError)
            {
                return Error(tokenResult.Error);
            }

            // pass scopes/claims to profile service
            var subject = tokenResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject).Value;
            var scopes = tokenResult.Claims.Where(c => c.Type == Constants.ClaimTypes.Scope).Select(c => c.Value);

            var payload = await _generator.ProcessAsync(subject, scopes);
            return new UserInfoResult(payload);
        }

        IHttpActionResult Error(string error, string description = null)
        {
            return new ProtectedResourceErrorResult(error, description);
        }
    }
}