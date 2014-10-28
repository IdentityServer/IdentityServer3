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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    public class DiscoveryEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;
        private readonly IScopeStore _scopes;

        public DiscoveryEndpointController(IdentityServerOptions options, IScopeStore scopes)
        {
            _options = options;
            _scopes = scopes;
        }

        [Route(Constants.RoutePaths.Oidc.DiscoveryConfiguration)]
        public async Task<IHttpActionResult> GetConfiguration()
        {
            Logger.Info("Start discovery request");

            if (!_options.Endpoints.DiscoveryEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var baseUrl = Request.GetIdentityServerBaseUrl();
            var scopes = await _scopes.GetScopesAsync();

            var supportedGrantTypes = Constants.SupportedGrantTypes.AsEnumerable();
            if (this._options.AuthenticationOptions.EnableLocalLogin == false)
            {
                supportedGrantTypes = supportedGrantTypes.Where(type => type != Constants.GrantTypes.Password);
            }

            return Json(new
            {
                issuer = _options.IssuerUri,
                jwks_uri = baseUrl + Constants.RoutePaths.Oidc.DiscoveryWebKeys,
                authorization_endpoint = baseUrl + Constants.RoutePaths.Oidc.Authorize,
                token_endpoint = baseUrl + Constants.RoutePaths.Oidc.Token,
                userinfo_endpoint = baseUrl + Constants.RoutePaths.Oidc.UserInfo,
                end_session_endpoint = baseUrl + Constants.RoutePaths.Oidc.EndSession,
                scopes_supported = scopes.Where(s => s.ShowInDiscoveryDocument).Select(s => s.Name),
                response_types_supported = Constants.SupportedResponseTypes,
                response_modes_supported = Constants.SupportedResponseModes,
                grant_types_supported = supportedGrantTypes,
                subject_types_supported = "public",
                id_token_signing_alg_values_supported = Constants.SigningAlgorithms.RSA_SHA_256
            });
        }

        [Route(Constants.RoutePaths.Oidc.DiscoveryWebKeys)]
        public IHttpActionResult GetKeyData()
        {
            Logger.Info("Start key discovery request");

            if (!_options.Endpoints.DiscoveryEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var webKeys = new List<JsonWebKeyDto>();
            foreach (var pubKey in _options.PublicKeysForMetadata)
            {
                if (pubKey != null)
                {
                    var cert64 = Convert.ToBase64String(pubKey.RawData);
                    var thumbprint = Base64Url.Encode(pubKey.GetCertHash());

                    var webKey = new JsonWebKeyDto
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = thumbprint,
                        x5t = thumbprint,
                        x5c = new[] { cert64 }
                    };

                    webKeys.Add(webKey);
                }
            }

            return Json(new { keys = webKeys });
        }

        private class JsonWebKeyDto
        {
            public string kty { get; set; }
            public string use { get; set; }
            public string kid { get; set; }
            public string x5t { get; set; }
            public string[] x5c { get; set; }
        }
    }
}