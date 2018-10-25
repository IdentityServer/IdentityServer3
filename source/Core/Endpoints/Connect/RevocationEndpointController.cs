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
    [NoCache]
    internal class RevocationEndpointController : ApiController
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly IEventService _events;
        private readonly ClientSecretValidator _clientValidator;
        private readonly TokenRevocationRequestValidator _requestValidator;
        private readonly ITokenRevocationService _tokenRevocationService;

        public RevocationEndpointController(ClientSecretValidator clientValidator, TokenRevocationRequestValidator requestValidator, IEventService events, ITokenRevocationService tokenRevocationService)
        {
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _events = events;
            _tokenRevocationService = tokenRevocationService;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start token revocation request");

            // validate client credentials and client
            var clientResult = await _clientValidator.ValidateAsync();
            if (clientResult.Client == null)
            {
                return new RevocationErrorResult(Constants.TokenErrors.InvalidClient);
            }

            var form = await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync();
            var response = await ProcessAsync(clientResult.Client, form);

            if (response is RevocationErrorResult)
            {
                var details = response as RevocationErrorResult;
                await RaiseFailureEventAsync(details.Error);
            }
            else
            {
                await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.Revocation);
            }

            Logger.Info("End token revocation request");
            return response;
        }

        public async Task<IHttpActionResult> ProcessAsync(Client client, NameValueCollection parameters)
        {
            // validate the token request
            var requestResult = await _requestValidator.ValidateRequestAsync(parameters, client);

            if (requestResult.IsError)
            {
                return new RevocationErrorResult(requestResult.Error);
            }

            var revocationResult = await _tokenRevocationService.RevokeAsync(requestResult);
            if (revocationResult.IsError)
            {
                return new RevocationErrorResult(revocationResult.Error);
            }

            return Ok();
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Revocation, error);
        }
    }
}