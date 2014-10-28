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
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [RoutePrefix(Constants.RoutePaths.Oidc.UserInfo)]
    [NoCache]
    public class UserInfoEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly UserInfoResponseGenerator _generator;
        private readonly TokenValidator _tokenValidator;
        private readonly IdentityServerOptions _options;

        public UserInfoEndpointController(IdentityServerOptions options, TokenValidator tokenValidator, UserInfoResponseGenerator generator)
        {
            _tokenValidator = tokenValidator;
            _generator = generator;
            _options = options;
        }

        [Route]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            Logger.Info("Start userinfo request");

            if (!_options.Endpoints.UserInfoEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var authorizationHeader = request.Headers.Authorization;

            if (authorizationHeader == null ||
                !authorizationHeader.Scheme.Equals(Constants.TokenTypes.Bearer) ||
                authorizationHeader.Parameter.IsMissing())
            {
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }

            var result = await _tokenValidator.ValidateAccessTokenAsync(
                authorizationHeader.Parameter, 
                Constants.StandardScopes.OpenId);

            if (result.IsError)
            {
                return Error(result.Error);
            }

            // pass scopes/claims to profile service
            var subject = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject).Value;
            var scopes = result.Claims.Where(c => c.Type == Constants.ClaimTypes.Scope).Select(c => c.Value);

            var payload = await _generator.ProcessAsync(subject, scopes);
            return new UserInfoResult(payload);
        }

        IHttpActionResult Error(string error, string description = null)
        {
            return new ProtectedResourceErrorResult(error, description);
        }
    }
}