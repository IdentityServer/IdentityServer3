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

using IdentityModel;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
    /// <summary>
    /// OpenID Connect discovery document endpoint
    /// </summary>
    internal class DiscoveryEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;
        private readonly IScopeStore _scopes;

        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public DiscoveryEndpointController(IdentityServerOptions options, IScopeStore scopes)
        {
            _options = options;
            _scopes = scopes;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns>Discovery document</returns>
        [Route(Constants.RoutePaths.Oidc.DiscoveryConfiguration)]
        public async Task<IHttpActionResult> GetConfiguration()
        {
            Logger.Info("Start discovery request");

            if (!_options.Endpoints.EnableDiscoveryEndpoint)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var baseUrl = Request.GetIdentityServerBaseUrl();
            var scopes = await _scopes.GetScopesAsync(publicOnly: true);

            var claims = new List<string>();
            foreach (var s in scopes)
            {
                claims.AddRange(from c in s.Claims 
                                where s.Type == ScopeType.Identity 
                                select c.Name);
            }

            var supportedGrantTypes = Constants.SupportedGrantTypes.AsEnumerable();
            if (this._options.AuthenticationOptions.EnableLocalLogin == false)
            {
                supportedGrantTypes = supportedGrantTypes.Where(type => type != Constants.GrantTypes.Password);
            }

            var dto = new DiscoveryDto
            {
                issuer = _options.IssuerUri,
                scopes_supported = scopes.Where(s => s.ShowInDiscoveryDocument).Select(s => s.Name).ToArray(),
                claims_supported = claims.Distinct().ToArray(),
                response_types_supported = Constants.SupportedResponseTypes.ToArray(),
                response_modes_supported = Constants.SupportedResponseModes.ToArray(),
                grant_types_supported = supportedGrantTypes.ToArray(),
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { Constants.SigningAlgorithms.RSA_SHA_256 },
                token_endpoint_auth_methods_supported = new[] { Constants.TokenEndpointAuthenticationMethods.PostBody, Constants.TokenEndpointAuthenticationMethods.BasicAuthentication }
            };

            if (_options.Endpoints.EnableAuthorizeEndpoint)
            {
                dto.authorization_endpoint = baseUrl + Constants.RoutePaths.Oidc.Authorize;
            }

            if (_options.Endpoints.EnableTokenEndpoint)
            {
                dto.token_endpoint = baseUrl + Constants.RoutePaths.Oidc.Token;
            }

            if (_options.Endpoints.EnableUserInfoEndpoint)
            {
                dto.userinfo_endpoint = baseUrl + Constants.RoutePaths.Oidc.UserInfo;
            }

            if (_options.Endpoints.EnableEndSessionEndpoint)
            {
                dto.end_session_endpoint = baseUrl + Constants.RoutePaths.Oidc.EndSession;
            }

            if (_options.Endpoints.EnableCheckSessionEndpoint)
            {
                dto.check_session_iframe = baseUrl + Constants.RoutePaths.Oidc.CheckSession;
            }

            if (_options.Endpoints.EnableTokenRevocationEndpoint)
            {
                dto.revocation_endpoint = baseUrl + Constants.RoutePaths.Oidc.Revocation;
            }

            if (_options.SigningCertificate != null)
            {
                dto.jwks_uri = baseUrl + Constants.RoutePaths.Oidc.DiscoveryWebKeys;
            }

            return Json(dto, Settings);
        }

        /// <summary>
        /// GET for JWKs
        /// </summary>
        /// <returns>JSON Web Key set</returns>
        [Route(Constants.RoutePaths.Oidc.DiscoveryWebKeys)]
        public IHttpActionResult GetKeyData()
        {
            Logger.Info("Start key discovery request");

            if (!_options.Endpoints.EnableDiscoveryEndpoint)
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
                    var key = pubKey.PublicKey.Key as RSACryptoServiceProvider;
                    var parameters = key.ExportParameters(false);
                    var exponent = Base64Url.Encode(parameters.Exponent);
                    var modulus = Base64Url.Encode(parameters.Modulus);

                    var webKey = new JsonWebKeyDto
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = thumbprint,
                        x5t = thumbprint,
                        e = exponent,
                        n = modulus,
                        x5c = new[] { cert64 }
                    };

                    webKeys.Add(webKey);
                }
            }

            return Json(new { keys = webKeys });
        }

        private class DiscoveryDto
        {
            public string issuer { get; set; }
            public string jwks_uri { get; set; }
            public string authorization_endpoint { get; set; }
            public string token_endpoint { get; set; }
            public string userinfo_endpoint { get; set; }
            public string end_session_endpoint { get; set; }
            public string check_session_iframe { get; set; }
            public string revocation_endpoint { get; set; }
            public string[] scopes_supported { get; set; }
            public string[] claims_supported { get; set; }
            public string[] response_types_supported { get; set; }
            public string[] response_modes_supported { get; set; }
            public string[] grant_types_supported { get; set; }
            public string[] subject_types_supported { get; set; }
            public string[] id_token_signing_alg_values_supported { get; set; }
            public string[] token_endpoint_auth_methods_supported { get; set; }
        };

        private class JsonWebKeyDto
        {
            public string kty { get; set; }
            public string use { get; set; }
            public string kid { get; set; }
            public string x5t { get; set; }
            public string e { get; set; }
            public string n { get; set; }
            public string[] x5c { get; set; }
        }
    }
}