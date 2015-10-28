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
using IdentityServer3.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default token signing service
    /// </summary>
    public class EnhancedDefaultTokenSigningService : ITokenSigningService
    {
        /// <summary>
        /// The identity server options
        /// </summary>
        protected readonly IdentityServerOptions _options;

        static EnhancedDefaultTokenSigningService()
        {
            JsonExtensions.Serializer = JsonConvert.SerializeObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenSigningService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public EnhancedDefaultTokenSigningService(IdentityServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Signs the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A protected and serialized security token
        /// </returns>
        public virtual async Task<string> SignTokenAsync(Token token)
        {
            var credentials = await GetSigningCredentialsAsync();
            return await CreateJsonWebToken(token, credentials);
        }

        /// <summary>
        /// Retrieves the signing credential (override to load key from alternative locations)
        /// </summary>
        /// <returns>The signing credential</returns>
        protected virtual Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            return Task.FromResult<SigningCredentials>(new X509SigningCredentials(_options.SigningCertificate));
        }

        /// <summary>
        /// Creates the json web token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The signed JWT</returns>
        protected virtual async Task<string> CreateJsonWebToken(Token token, SigningCredentials credentials)
        {
            var header = CreateHeader(token, credentials);
            var payload = CreatePayload(token);

            return await SignAsync(new JwtSecurityToken(header, payload));
        }

        /// <summary>
        /// Creates the JWT header
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="credential">The credentials.</param>
        /// <returns>The JWT header</returns>
        protected virtual JwtHeader CreateHeader(Token token, SigningCredentials credential)
        {
            var header = new JwtHeader(credential);

            var x509credential = credential as X509SigningCredentials;
            if (x509credential != null)
            {
                header.Add("kid", Base64Url.Encode(x509credential.Certificate.GetCertHash()));
            }

            return header;
        }

        /// <summary>
        /// Creates the JWT payload
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The JWT payload</returns>
        protected virtual JwtPayload CreatePayload(Token token)
        {
            var payload = new JwtPayload(
                token.Issuer,
                token.Audience,
                null,
                DateTimeHelper.UtcNow,
                DateTimeHelper.UtcNow.AddSeconds(token.Lifetime));

            var amr = new HashSet<string>();
            var jsonObjects = new Dictionary<string, List<JObject>>();
            var jsonArrays = new Dictionary<string, JArray>();

            foreach (var claim in token.Claims)
            {
                if (claim.Type == "amr")
                {
                    amr.Add(claim.Value);
                    continue;
                }
                if (claim.ValueType == "JsonArray")
                {
                    var array = JArray.Parse(claim.Value);
                    if (payload.ContainsKey(claim.Type))
                    {
                        throw new Exception("Can't add two JSON array claims of the same type");
                    }
                    else
                    {
                        payload.Add(claim.Type, array);
                    }

                    continue;
                }
                if (claim.ValueType == "JsonObject")
                {
                    var obj = JObject.Parse(claim.Value);

                    if (jsonObjects.ContainsKey(claim.Type))
                    {
                        jsonObjects[claim.Type].Add(obj);
                    }
                    else
                    {
                        jsonObjects.Add(claim.Type, new List<JObject> { obj });
                    }

                    continue;
                }

                payload.AddClaim(claim);
            }

            if (amr.Any())
            {
                payload.Add("amr", amr.ToArray());
            }

            if (jsonObjects.Any())
            {
                foreach (var key in jsonObjects.Keys)
                {
                    var item = jsonObjects[key];
                    if (item.Count() == 1)
                    {
                        payload.Add(key, item.First());
                    }
                    else
                    {
                        payload.Add(key, item.ToArray());
                    }
                }
            }

            return payload;
        }

        /// <summary>
        /// Applies the signature to the JWT
        /// </summary>
        /// <param name="jwt">The JWT object.</param>
        /// <returns>The signed JWT</returns>
        protected virtual Task<string> SignAsync(JwtSecurityToken jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            return Task.FromResult(handler.WriteToken(jwt));
        }
    }
}