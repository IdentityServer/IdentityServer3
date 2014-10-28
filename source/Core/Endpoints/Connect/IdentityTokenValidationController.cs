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

using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [RoutePrefix(Constants.RoutePaths.Oidc.IdentityTokenValidation)]
    [NoCache]
    public class IdentityTokenValidationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly TokenValidator _validator;
        private readonly IdentityServerOptions _options;

        public IdentityTokenValidationController(TokenValidator validator, IdentityServerOptions options)
        {
            _validator = validator;
            _options = options;
        }

        [Route]
        public async Task<IHttpActionResult> Get()
        {
            Logger.Info("Start identity token validation request");

            if (!_options.Endpoints.IdentityTokenValidationEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var parameters = Request.RequestUri.ParseQueryString();

            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                Logger.Error("token is missing.");
                return BadRequest(Messages.MissingToken);
            }

            var clientId = parameters.Get("client_id");
            if (clientId.IsMissing())
            {
                Logger.Error("client_id is missing.");
                return BadRequest(Messages.MissingClientId);
            }

            var result = await _validator.ValidateIdentityTokenAsync(token, clientId);
            
            if (result.IsError)
            {
                Logger.Info("Returning error: " + result.Error);
                return BadRequest(result.Error);
            }

            var response = result.Claims.ToClaimsDictionary();
            Logger.Debug(JsonConvert.SerializeObject(response, Formatting.Indented));

            Logger.Info("Returning identity token claims");
            return Json(response);
        }
    }
}