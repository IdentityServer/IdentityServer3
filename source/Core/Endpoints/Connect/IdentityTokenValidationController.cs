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
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    /// <summary>
    /// Endpoint for validating identity tokens
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
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
                RaiseFailureEvent(error);

                return NotFound();
            }

            var parameters = Request.RequestUri.ParseQueryString();

            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                var error = "token is missing.";
                Logger.Error(error);
                RaiseFailureEvent(error);

                return BadRequest(_localizationService.GetMessage(MessageIds.MissingToken));
            }

            var clientId = parameters.Get("client_id");
            if (clientId.IsMissing())
            {
                var error = "client_id is missing.";
                Logger.Error(error);
                RaiseFailureEvent(error);

                return BadRequest(_localizationService.GetMessage(MessageIds.MissingClientId));
            }

            var result = await _validator.ValidateIdentityTokenAsync(token, clientId);

            if (result.IsError)
            {
                Logger.Info("Returning error: " + result.Error);
                RaiseFailureEvent(result.Error);

                return BadRequest(result.Error);
            }

            var response = result.Claims.ToClaimsDictionary();

            Logger.Info("End identity token validation request");
            RaiseSuccessEvent();

            return Json(response);
        }

        private void RaiseSuccessEvent()
        {
            _events.RaiseSuccessfulEndpointEvent(EventConstants.EndpointNames.IdentityTokenValidation);
        }

        private void RaiseFailureEvent(string error)
        {
            _events.RaiseFailureEndpointEvent(EventConstants.EndpointNames.IdentityTokenValidation, error);
        }
    }
}