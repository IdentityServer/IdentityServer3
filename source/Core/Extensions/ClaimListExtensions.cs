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
using IdentityServer3.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer3.Core.Extensions
{
    internal static class ClaimListExtensions
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public static Dictionary<string, object> ToClaimsDictionary(this IEnumerable<Claim> claims)
        {
            var d = new Dictionary<string, object>();

            if (claims == null)
            {
                return d;
            }

            var distinctClaims = claims.Distinct(new ClaimComparer());

            foreach (var claim in distinctClaims)
            {
                if (!d.ContainsKey(claim.Type))
                {
                    d.Add(claim.Type, GetValue(claim));
                }
                else
                {
                    var value = d[claim.Type];

                    var list = value as List<object>;
                    if (list != null)
                    {
                        list.Add(GetValue(claim));
                    }
                    else
                    {
                        d.Remove(claim.Type);
                        d.Add(claim.Type, new List<object> { value, GetValue(claim) });
                    }
                }
            }

            return d;
        }

        private static object GetValue(Claim claim)
        {
            if (claim.Type == Constants.ClaimTypes.Address)
            {
                try
                {
                    return JsonConvert.DeserializeObject(claim.Value);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Exception while deserializing address claim", ex);
                }
            }

            if (claim.ValueType == ClaimValueTypes.Integer ||
                claim.ValueType == ClaimValueTypes.Integer32)
            {
                Int32 value;
                if (Int32.TryParse(claim.Value, out value))
                {
                    return value;
                }
            }

            if (claim.ValueType == ClaimValueTypes.Integer64)
            {
                Int64 value;
                if (Int64.TryParse(claim.Value, out value))
                {
                    return value;
                }
            }

            if (claim.ValueType == ClaimValueTypes.Boolean)
            {
                bool value;
                if (bool.TryParse(claim.Value, out value))
                {
                    return value;
                }
            }

            return claim.Value;
        }
    }
}