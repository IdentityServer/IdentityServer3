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
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OAuth2/OpenID Conect token endpoint
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [RoutePrefix(Constants.RoutePaths.Oidc.Token)]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class TokenEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly TokenResponseGenerator _generator;
        private readonly TokenRequestValidator _requestValidator;
        private readonly ClientValidator _clientValidator;
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
        public TokenEndpointController(IdentityServerOptions options, TokenRequestValidator requestValidator, ClientValidator clientValidator, TokenResponseGenerator generator, IEventService events)
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
                RaiseFailureEvent(error);

                return NotFound();
            }

            var response = await ProcessAsync(await Request.Content.ReadAsFormDataAsync());

            if (response is TokenErrorResult)
            {
                var details = response as TokenErrorResult;
                RaiseFailureEvent(details.Error);
            }
            else
            {
                _events.RaiseSuccessfulEndpointEvent(EventConstants.EndpointNames.Token);
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
            var client = await _clientValidator.ValidateClientAsync(parameters, Request.Headers.Authorization);
            if (client == null)
            {
                return this.TokenErrorResponse(Constants.TokenErrors.InvalidClient);
            }

            // validate the token request
            var result = await _requestValidator.ValidateRequestAsync(parameters, client);

            if (result.IsError)
            {
                return this.TokenErrorResponse(result.Error);
            }

            // return response
            var response = await _generator.ProcessAsync(_requestValidator.ValidatedRequest);
            return this.TokenResponse(response);
        }

        private void RaiseFailureEvent(string error)
        {
            _events.RaiseFailureEndpointEvent(EventConstants.EndpointNames.Token, error);
        }
    }
}