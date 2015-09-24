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
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Results;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
    /// <summary>
    /// Implementation of RFC 7009 (http://tools.ietf.org/html/rfc7009)
    /// </summary>
    [RoutePrefix(Constants.RoutePaths.Oidc.Revocation)]
    [NoCache]
    internal class RevocationEndpointController : ApiController
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly IEventService _events;
        private readonly ClientSecretValidator _clientValidator;
        private readonly IdentityServerOptions _options;
        private readonly TokenRevocationRequestValidator _requestValidator;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly IRefreshTokenStore _refreshTokens;

        public RevocationEndpointController(IdentityServerOptions options, ClientSecretValidator clientValidator, TokenRevocationRequestValidator requestValidator, ITokenHandleStore tokenHandles, IRefreshTokenStore refreshTokens, IEventService events)
        {
            _options = options;
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _tokenHandles = tokenHandles;
            _refreshTokens = refreshTokens;
            _events = events;
        }

        [Route]
        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start token revocation request");

            if (!_options.Endpoints.EnableTokenRevocationEndpoint)
            {
                var error = "Endpoint is disabled. Aborting";
                Logger.Warn(error);
                await RaiseFailureEventAsync(error);

                return NotFound();
            }

            var response = await ProcessAsync(await Request.Content.ReadAsFormDataAsync());

            if (response is RevocationErrorResult)
            {
                var details = response as RevocationErrorResult;
                await RaiseFailureEventAsync(details.Error);
            }
            else
            {
                await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.Token);
            }

            Logger.Info("End token revocation request");
            return response;
        }

        public async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            // validate client credentials and client
            var clientResult = await _clientValidator.ValidateAsync();
            if (clientResult.Client == null)
            {
                return new RevocationErrorResult(Constants.TokenErrors.InvalidClient);
            }

            // validate the token request
            var requestResult = await _requestValidator.ValidateRequestAsync(parameters, clientResult.Client);

            if (requestResult.IsError)
            {
                return new RevocationErrorResult(requestResult.Error);
            }

            // revoke tokens
            if (requestResult.TokenTypeHint == Constants.TokenTypeHints.AccessToken)
            {
                await RevokeAccessTokenAsync(requestResult.Token, clientResult.Client);
            }
            else if (requestResult.TokenTypeHint == Constants.TokenTypeHints.RefreshToken)
            {
                await RevokeRefreshTokenAsync(requestResult.Token, clientResult.Client);
            }
            else
            {
                var found = await RevokeAccessTokenAsync(requestResult.Token, clientResult.Client);

                if (!found)
                {
                    await RevokeRefreshTokenAsync(requestResult.Token, clientResult.Client);
                }
            }

            return Ok();
        }

        // revoke access token only if it belongs to client doing the request
        private async Task<bool> RevokeAccessTokenAsync(string handle, Client client)
        {
            var token = await _tokenHandles.GetAsync(handle);

            if (token != null)
            {
                if (token.ClientId == client.ClientId)
                {
                    await _tokenHandles.RemoveAsync(handle);
                }
                else
                {
                    var message = string.Format("Client {0} tried to revoke an access token belonging to a different client: {1}", client.ClientId, token.ClientId);

                    Logger.Warn(message);
                    await RaiseFailureEventAsync(message);
                }

                return true;
            }

            return false;
        }

        // revoke refresh token only if it belongs to client doing the request
        private async Task<bool> RevokeRefreshTokenAsync(string handle, Client client)
        {
            var token = await _refreshTokens.GetAsync(handle);

            if (token != null)
            {
                if (token.ClientId == client.ClientId)
                {
                    await _refreshTokens.RevokeAsync(token.SubjectId, token.ClientId);
                    await _tokenHandles.RevokeAsync(token.SubjectId, token.ClientId);
                }
                else
                {
                    var message = string.Format("Client {0} tried to revoke a refresh token belonging to a different client: {1}", client.ClientId, token.ClientId);
                    
                    Logger.Warn(message);
                    await RaiseFailureEventAsync(message);
                }

                return true;
            }

            return false;
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Revocation, error);
        }
    }
}