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


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Extension methods for Token
    /// </summary>
    public static class TokenExtensions
    {
        static TokenExtensions()
        {
            JsonExtensions.Serializer = JsonConvert.SerializeObject;
        }

        /// <summary>
        /// Turns a token object into JWT payload
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>Serialized JWT payload</returns>
        public static string CreateJwtPayload(this Token token)
        {
            var payload = new JwtPayload(
                token.Issuer,
                token.Audience,
                null,
                token.CreationTime.UtcDateTime,
                token.CreationTime.AddSeconds(token.Lifetime).UtcDateTime);

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
            var jsonObjectGroups = jsonObjects.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonObjectGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON object and the other is not a JSON object ({0})", group.Key));
                }

                if (group.Skip(1).Any())
                {
                    // add as array
                    payload.Add(group.Key, group.Select(x => x.JsonValue).ToArray());
                }
                else
                {
                    // add just one
                    payload.Add(group.Key, group.First().JsonValue);
                }
            }

            var jsonArrays = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Array).ToArray();
            var jsonArrayGroups = jsonArrays.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonArrayGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON array and the other is not a JSON array ({0})", group.Key));
                }

                List<JToken> newArr = new List<JToken>();
                foreach (var arrays in group)
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
    }
}