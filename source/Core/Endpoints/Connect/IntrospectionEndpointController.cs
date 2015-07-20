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
    /// Endpoint for token introspection - see https://tools.ietf.org/html/draft-ietf-oauth-introspection-11
    /// </summary>
    [RoutePrefix(Constants.RoutePaths.Oidc.Introspection)]
    [NoCache]
    internal class IntrospectionEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly ILocalizationService _localizationService;
        private readonly IEventService _events;
        private readonly ScopeSecretValidator _scopeValidator;
        private readonly IntrospectionRequestValidator _requestValidator;

        public IntrospectionEndpointController(
            IntrospectionRequestValidator requestValidator, 
            IdentityServerOptions options, 
            ILocalizationService localizationService, 
            IEventService events,
            ScopeSecretValidator scopeValidator)
        {
            _requestValidator = requestValidator;
            _scopeValidator = scopeValidator;
            _options = options;
            _localizationService = localizationService;
            _events = events;
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <returns>Introspection response</returns>
        [Route]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start introspection request");

            if (!_options.Endpoints.EnableIntrospectionEndpoint)
            {
                var error = "Endpoint is disabled. Aborting";
                Logger.Warn(error);
                await RaiseFailureEventAsync(error);

                return NotFound();
            }

            var scope = await _scopeValidator.ValidateAsync();
            if (scope == null)
            {
                // logging
                return Unauthorized();
            }

            var parameters = await Request.GetOwinContext().ReadRequestFormAsNameValueCollectionAsync();
            return await ProcessRequest(parameters, scope.Scope);
        }

        internal async Task<IHttpActionResult> ProcessRequest(NameValueCollection parameters, Scope scope)
        {
            var validationResult = await _requestValidator.ValidateAsync(parameters, scope);

            if (validationResult.IsActive)
            {
                var response = validationResult.Claims.ToClaimsDictionary();
                response.Add("active", true);

                return Json(response);
            }
            else if(validationResult.IsActive == false)
            {
                return Json(new { active = false });
            }
            else
            {
                Logger.Error(validationResult.ErrorDescription);
                
                await RaiseFailureEventAsync(validationResult.ErrorDescription);
                return BadRequest(_localizationService.GetMessage(validationResult.ErrorDescription));
            }

            //var token = parameters.Get("token");
            //if (token.IsMissing())
            //{
            //    var error = "token is missing";

            //    Logger.Error(error);
            //    await RaiseFailureEventAsync(error);
            //    return BadRequest(_localizationService.GetMessage(MessageIds.MissingToken));
            //}

            //var result = await _tokenValidator.ValidateAccessTokenAsync(token, parameters.Get("expectedScope"));

            //if (result.IsError)
            //{
            //    Logger.Info("Returning error: " + result.Error);
            //    await RaiseFailureEventAsync(result.Error);

            //    return BadRequest(result.Error);
            //}

            //var response = result.Claims.ToClaimsDictionary();

            //Logger.Info("End access token validation request");
            //await RaiseSuccessEventAsync();

            //return Json(response);
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.AccessTokenValidation);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.AccessTokenValidation, error);
        }
    }
}