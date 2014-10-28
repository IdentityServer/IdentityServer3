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

using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [RoutePrefix(Constants.RoutePaths.Oidc.Token)]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    public class TokenEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly TokenResponseGenerator _generator;
        private readonly TokenRequestValidator _requestValidator;
        private readonly ClientValidator _clientValidator;
        private readonly IdentityServerOptions _options;

        public TokenEndpointController(IdentityServerOptions options, TokenRequestValidator requestValidator, ClientValidator clientValidator, TokenResponseGenerator generator)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _generator = generator;
            _options = options;
        }

        [Route]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start token request");

            return await ProcessAsync(await Request.Content.ReadAsFormDataAsync());
        }

        public async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            if (!_options.Endpoints.TokenEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

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
    }
}