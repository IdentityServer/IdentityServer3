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
using IdentityServer3.Core.Validation;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly CustomGrantValidator _customGrants;
        private readonly IOwinContext _context;
        private readonly ISigningKeyService _keyService;

        static readonly JsonSerializer Serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public DiscoveryEndpointController(IdentityServerOptions options, IScopeStore scopes, IOwinContext context, ISigningKeyService keyService, CustomGrantValidator customGrants)
        {
            _options = options;
            _scopes = scopes;
            _context = context;
            _keyService = keyService;
            _customGrants = customGrants;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <returns>Discovery document</returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetConfiguration()
        {
            Logger.Info("Start discovery request");

            var baseUrl = Request.GetIdentityServerBaseUrl();
            var allScopes = await _scopes.GetScopesAsync(publicOnly: true);
            var showScopes = new List<Scope>();

            var dto = new DiscoveryDto
            {
                issuer = _context.GetIdentityServerIssuerUri(),
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { Constants.SigningAlgorithms.RSA_SHA_256 },
                code_challenge_methods_supported = new[] { Constants.CodeChallengeMethods.Plain, Constants.CodeChallengeMethods.SHA_256 }
            };
            
            // scopes
            if (_options.DiscoveryOptions.ShowIdentityScopes)
            {
                showScopes.AddRange(allScopes.Where(s => s.Type == ScopeType.Identity));
            }
            if (_options.DiscoveryOptions.ShowResourceScopes)
            {
                showScopes.AddRange(allScopes.Where(s => s.Type == ScopeType.Resource));
            }

            if (showScopes.Any())
            {
                dto.scopes_supported = showScopes.Where(s => s.ShowInDiscoveryDocument).Select(s => s.Name).ToArray();
            }

            // claims
            if (_options.DiscoveryOptions.ShowClaims)
            {
                var claims = new List<string>();
                foreach (var s in allScopes)
                {
                    claims.AddRange(from c in s.Claims
                                    where s.Type == ScopeType.Identity
                                    select c.Name);
                }

                dto.claims_supported = claims.Distinct().ToArray();
            }

            // grant types
            if (_options.DiscoveryOptions.ShowGrantTypes)
            {
                var standardGrantTypes = Constants.SupportedGrantTypes.AsEnumerable();
                if (this._options.AuthenticationOptions.EnableLocalLogin == false)
                {
                    standardGrantTypes = standardGrantTypes.Where(type => type != Constants.GrantTypes.Password);
                }

                var showGrantTypes = new List<string>(standardGrantTypes);

                if (_options.DiscoveryOptions.ShowCustomGrantTypes)
                {
                    showGrantTypes.AddRange(_customGrants.GetAvailableGrantTypes());
                }

                dto.grant_types_supported = showGrantTypes.ToArray();
            }

            // response types
            if (_options.DiscoveryOptions.ShowResponseTypes)
            {
                dto.response_types_supported = Constants.SupportedResponseTypes.ToArray();
            }

            // response modes
            if (_options.DiscoveryOptions.ShowResponseModes)
            {
                dto.response_modes_supported = Constants.SupportedResponseModes.ToArray();
            }

            // token endpoint authentication methods
            if (_options.DiscoveryOptions.ShowTokenEndpointAuthenticationMethods)
            {
                dto.token_endpoint_auth_methods_supported = new[] { Constants.TokenEndpointAuthenticationMethods.PostBody, Constants.TokenEndpointAuthenticationMethods.BasicAuthentication };
            }

            // endpoints
            if (_options.DiscoveryOptions.ShowEndpoints)
            {
                if (_options.Endpoints.EnableEndSessionEndpoint)
                {
                    dto.frontchannel_logout_supported = true;
                    dto.frontchannel_logout_session_supported = true;
                }

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

                if (_options.Endpoints.EnableIntrospectionEndpoint)
                {
                    dto.introspection_endpoint = baseUrl + Constants.RoutePaths.Oidc.Introspection;
                }
            }

            if (_options.DiscoveryOptions.ShowKeySet)
            {
                if (_options.SigningCertificate != null)
                {
                    dto.jwks_uri = baseUrl + Constants.RoutePaths.Oidc.DiscoveryWebKeys;
                }
            }

            var jobject = JObject.FromObject(dto, Serializer);

            // custom entries
            if (_options.DiscoveryOptions.CustomEntries != null && _options.DiscoveryOptions.CustomEntries.Any())
            {
                foreach (var item in _options.DiscoveryOptions.CustomEntries)
                {
                    JToken token;
                    if (jobject.TryGetValue(item.Key, out token))
                    {
                        throw new Exception("Item does already exist - cannot add it via a custom entry: " + item.Key);
                    }

                    jobject.Add(new JProperty(item.Key, item.Value));
                }
            }

            return Content(HttpStatusCode.OK, jobject);
        }

        /// <summary>
        /// GET for JWKs
        /// </summary>
        /// <returns>JSON Web Key set</returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetKeyData()
        {
            Logger.Info("Start key discovery request");

            if (_options.DiscoveryOptions.ShowKeySet == false)
            {
                Logger.Info("Key discovery disabled. 404.");
                return NotFound();
            }

            var webKeys = new List<JsonWebKeyDto>();
            foreach (var pubKey in await _keyService.GetPublicKeysAsync())
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
                        kid = await _keyService.GetKidAsync(pubKey),
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
            public string introspection_endpoint { get; set; }
            public bool? frontchannel_logout_supported { get; set; }
            public bool? frontchannel_logout_session_supported { get; set; }
            public string[] scopes_supported { get; set; }
            public string[] claims_supported { get; set; }
            public string[] response_types_supported { get; set; }
            public string[] response_modes_supported { get; set; }
            public string[] grant_types_supported { get; set; }
            public string[] subject_types_supported { get; set; }
            public string[] id_token_signing_alg_values_supported { get; set; }
            public string[] code_challenge_methods_supported { get; set; }
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