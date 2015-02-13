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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    /// <summary>
    /// Implementation of RFC 7009 (http://tools.ietf.org/html/rfc7009)
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [RoutePrefix(Constants.RoutePaths.Oidc.Revocation)]
    [NoCache]
    internal class RevocationEndpointController : ApiController
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly IEventService _events;
        private readonly ClientValidator _clientValidator;
        private readonly IdentityServerOptions _options;
        private readonly TokenRevocationRequestValidator _requestValidator;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly IRefreshTokenStore _refreshTokens;

        public RevocationEndpointController(IdentityServerOptions options, ClientValidator clientValidator, TokenRevocationRequestValidator requestValidator, ITokenHandleStore tokenHandles, IRefreshTokenStore refreshTokens, IEventService events)
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
                RaiseFailureEvent(error);

                return NotFound();
            }

            var response = await ProcessAsync(await Request.Content.ReadAsFormDataAsync());

            if (response is RevocationErrorResult)
            {
                var details = response as RevocationErrorResult;
                RaiseFailureEvent(details.Error);
            }
            else
            {
                _events.RaiseSuccessfulEndpointEvent(EventConstants.EndpointNames.Token);
            }

            Logger.Info("End token revocation request");
            return response;
        }

        public async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            // validate client credentials and client
            var client = await _clientValidator.ValidateClientAsync(parameters, Request.Headers.Authorization);
            if (client == null)
            {
                return new RevocationErrorResult(Constants.TokenErrors.InvalidClient);
            }

            // validate the token request
            var result = await _requestValidator.ValidateRequestAsync(parameters, client);

            if (result.IsError)
            {
                return new RevocationErrorResult(result.Error);
            }

            // revoke tokens
            if (result.TokenTypeHint == Constants.TokenTypeHints.AccessToken)
            {
                await RevokeAccessTokenAsync(result.Token, client);
            }
            else if (result.TokenTypeHint == Constants.TokenTypeHints.RefreshToken)
            {
                await RevokeRefreshTokenAsync(result.Token, client);
            }
            else
            {
                var found = await RevokeAccessTokenAsync(result.Token, client);

                if (!found)
                {
                    await RevokeRefreshTokenAsync(result.Token, client);
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
                    RaiseFailureEvent(message);
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
                    await _refreshTokens.RemoveAsync(handle);
                }
                else
                {
                    var message = string.Format("Client {0} tried to revoke a refresh token belonging to a different client: {1}", client.ClientId, token.ClientId);
                    
                    Logger.Warn(message);
                    RaiseFailureEvent(message);
                }

                return true;
            }

            return false;
        }

        private void RaiseFailureEvent(string error)
        {
            _events.RaiseFailureEndpointEvent(EventConstants.EndpointNames.Revocation, error);
        }
    }
}