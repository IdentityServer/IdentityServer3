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
using IdentityServer3.Core.Resources;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
    /// <summary>
    /// Endpoint for validating identity tokens
    /// </summary>
    [RoutePrefix(Constants.RoutePaths.Oidc.IdentityTokenValidation)]
    [NoCache]
    internal class IdentityTokenValidationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly TokenValidator _validator;
        private readonly IdentityServerOptions _options;
        private readonly ILocalizationService _localizationService;
        private readonly IEventService _events;

        public IdentityTokenValidationController(TokenValidator validator, IdentityServerOptions options, ILocalizationService localizationService, IEventService events)
        {
            _validator = validator;
            _options = options;
            _localizationService = localizationService;
            _events = events;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns>Claims if token is valid</returns>
        [Route]
        public async Task<IHttpActionResult> Get()
        {
            Logger.Info("Start identity token validation request");

            if (!_options.Endpoints.EnableIdentityTokenValidationEndpoint)
            {
                var error = "Endpoint is disabled. Aborting";
                Logger.Warn(error);
                await RaiseFailureEventAsync(error);

                return NotFound();
            }

            var parameters = Request.RequestUri.ParseQueryString();
            return await ProcessAsync(parameters);
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <returns>Claims if token is valid</returns>
        [Route]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start identity token validation request");

            if (!_options.Endpoints.EnableIdentityTokenValidationEndpoint)
            {
                var error = "Endpoint is disabled. Aborting";
                Logger.Warn(error);
                await RaiseFailureEventAsync(error);

                return NotFound();
            }

            var parameters = await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync();
            return await ProcessAsync(parameters);
        }

        internal async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                var error = "token is missing.";
                Logger.Error(error);
                await RaiseFailureEventAsync(error);

                return BadRequest(_localizationService.GetMessage(MessageIds.MissingToken));
            }

            var clientId = parameters.Get("client_id");
            if (clientId.IsMissing())
            {
                var error = "client_id is missing.";
                Logger.Error(error);
                await RaiseFailureEventAsync(error);

                return BadRequest(_localizationService.GetMessage(MessageIds.MissingClientId));
            }

            var result = await _validator.ValidateIdentityTokenAsync(token, clientId);

            if (result.IsError)
            {
                Logger.Info("Returning error: " + result.Error);
                await RaiseFailureEventAsync(result.Error);

                return BadRequest(result.Error);
            }

            var response = result.Claims.ToClaimsDictionary();

            Logger.Info("End identity token validation request");
            await RaiseSuccessEventAsync();

            return Json(response);
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.IdentityTokenValidation);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.IdentityTokenValidation, error);
        }
    }
}