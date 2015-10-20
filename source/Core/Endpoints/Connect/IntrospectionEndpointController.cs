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
    [NoCache]
    internal class IntrospectionEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly IEventService _events;
        private readonly ScopeSecretValidator _scopeSecretValidator;
        private readonly IntrospectionRequestValidator _requestValidator;

        public IntrospectionEndpointController(
            IntrospectionRequestValidator requestValidator, 
            IdentityServerOptions options, 
            IEventService events,
            ScopeSecretValidator scopeSecretValidator)
        {
            _requestValidator = requestValidator;
            _scopeSecretValidator = scopeSecretValidator;
            _options = options;
            _events = events;
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <returns>Introspection response</returns>
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start introspection request");

            var scope = await _scopeSecretValidator.ValidateAsync();
            if (scope.Scope == null)
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
                response.Add("scope", scope.Name);

                return Json(response);
            }
            else if(validationResult.IsError == true)
            {
                Logger.Error(validationResult.ErrorDescription);

                await RaiseFailureEventAsync(validationResult.ErrorDescription);
                return BadRequest(validationResult.ErrorDescription);
            }
            else
            {
                return Json(new { active = false });
            }
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