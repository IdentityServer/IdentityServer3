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
    public class DefaultTokenSigningService : ITokenSigningService
    {
        /// <summary>
        /// The identity server options
        /// </summary>
        protected readonly IdentityServerOptions _options;

        static DefaultTokenSigningService()
        {
            JsonExtensions.Serializer = JsonConvert.SerializeObject;
            //JsonExtensions.Deserializer = JsonConvert.DeserializeObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenSigningService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DefaultTokenSigningService(IdentityServerOptions options)
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
            var payload = CreatePayload(token);
            return await SignAsync(payload, credentials);
        }

        /// <summary>
        /// Creates the JWT payload
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The JWT payload</returns>
        protected virtual string CreatePayload(Token token)
        {
            var payload = new JwtPayload(
                token.Issuer,
                token.Audience,
                null,
                DateTimeHelper.UtcNow,
                DateTimeHelper.UtcNow.AddSeconds(token.Lifetime));

            var amrClaims = token.Claims.Where(x => x.Type == Constants.ClaimTypes.AuthenticationMethod);
            var jsonClaims = token.Claims.Where(x => x.ValueType == Constants.ClaimValueTypes.Json);
            var normalClaims = token.Claims.Except(amrClaims).Except(jsonClaims);

            payload.AddClaims(normalClaims);

            // deal with amr
            var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
            if (amrValues.Any())
            {
                payload.Add(Constants.ClaimTypes.AuthenticationMethod, amrValues);
            }

            // deal with json types
            // calling ToArray() to trigger JSON parsing once and so later 
            // collection identity comparisons work for the anonymous type
            var jsonTokens = jsonClaims.Select(x => new { x.Type, JsonValue = JRaw.Parse(x.Value) }).ToArray();

            var jsonObjects = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Object).ToArray();
            var jsonObjectGroups = jsonObjects.GroupBy(x=>x.Type).ToArray();
            foreach(var group in jsonObjectGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON object and the other is not a JSON object ({0})", group.Key));
                }

                if (group.Skip(1).Any())
                {
                    // add as array
                    payload.Add(group.Key, group.Select(x=>x.JsonValue).ToArray());
                }
                else
                {
                    // add just one
                    payload.Add(group.Key, group.First().JsonValue);
                }
            }

            var jsonArrays = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Array).ToArray();
            var jsonArrayGroups = jsonArrays.GroupBy(x=>x.Type).ToArray();
            foreach (var group in jsonArrayGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON array and the other is not a JSON array ({0})", group.Key));
                }

                List<JToken> newArr = new List<JToken>();
                foreach(var arrays in group)
                {
                    var arr = (JArray)arrays.JsonValue;
                    newArr.AddRange(arr);
                }

                // add just one array for the group/key/claim type
                payload.Add(group.Key, newArr.ToArray());
            }

            var unsupportedJsonTokens = jsonTokens.Except(jsonObjects).Except(jsonArrays);
            var unsupportedJsonClaimTypes = unsupportedJsonTokens.Select(x => x.Type).Distinct();
            if (unsupportedJsonClaimTypes.Any())
            {
                throw new Exception(String.Format("Unsupported JSON type for claim types: {0}", unsupportedJsonClaimTypes.Aggregate((x, y) => x + ", " + y)));
            }

            return payload.SerializeToJson();
        }

        /// <summary>
        /// Creates the JWT header
        /// </summary>
        /// <param name="credential">The credentials.</param>
        /// <returns>The JWT header</returns>
        private JwtHeader CreateHeader(SigningCredentials credential)
        {
            var header = new JwtHeader(credential);

            var x509credential = credential as X509SigningCredentials;
            if (x509credential != null)
            {
                header.Add("kid", Base64Url.Encode(x509credential.Certificate.GetCertHash()));
            }

            return header;
        }

        private Task<string> SignAsync(string payload, SigningCredentials credentials)
        {
            var header = CreateHeader(credentials);
            var jwtPayload = JwtPayload.Deserialize(payload);

            var token = new JwtSecurityToken(header, jwtPayload);

            var handler = new JwtSecurityTokenHandler();
            return Task.FromResult(handler.WriteToken(token));
        }
    }
}