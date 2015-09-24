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
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
    /// <summary>
    /// OAuth2/OpenID Conect token endpoint
    /// </summary>
    [RoutePrefix(Constants.RoutePaths.Oidc.Token)]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class TokenEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly TokenResponseGenerator _generator;
        private readonly TokenRequestValidator _requestValidator;
        private readonly ClientSecretValidator _clientValidator;
        private readonly IdentityServerOptions _options;
        private readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenEndpointController" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="clientValidator">The client validator.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="events">The events service.</param>
        public TokenEndpointController(IdentityServerOptions options, TokenRequestValidator requestValidator, ClientSecretValidator clientValidator, TokenResponseGenerator generator, IEventService events)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _generator = generator;
            _options = options;
            _events = events;
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <returns>Token response</returns>
        [Route]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start token request");

            if (!_options.Endpoints.EnableTokenEndpoint)
            {
                var error = "Endpoint is disabled. Aborting";
                Logger.Warn(error);
                await RaiseFailureEventAsync(error);

                return NotFound();
            }

            var response = await ProcessAsync(await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync());
            
            if (response is TokenErrorResult)
            {
                var details = response as TokenErrorResult;
                await RaiseFailureEventAsync(details.Error);
            }
            else
            {
                await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.Token);
            }

            Logger.Info("End token request");
            return response;
        }

        /// <summary>
        /// Processes the token request
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Token response</returns>
        public async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            // validate client credentials and client
            var clientResult = await _clientValidator.ValidateAsync();
            if (clientResult.IsError)
            {
                return this.TokenErrorResponse(Constants.TokenErrors.InvalidClient);
            }

            // validate the token request
            var requestResult = await _requestValidator.ValidateRequestAsync(parameters, clientResult.Client);

            if (requestResult.IsError)
            {
                return this.TokenErrorResponse(requestResult.Error, requestResult.ErrorDescription);
            }

            // return response
            var response = await _generator.ProcessAsync(_requestValidator.ValidatedRequest);
            return this.TokenResponse(response);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Token, error);
        }
    }
}