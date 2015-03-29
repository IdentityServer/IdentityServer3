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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Claims filter for facebook. Converts the "urn:facebook:name" claim to the "name" claim.
    /// </summary>
    public class FacebookClaimsFilter : ClaimsFilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClaimsFilter"/> class.
        /// </summary>
        public FacebookClaimsFilter()
            : this("Facebook")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClaimsFilter"/> class.
        /// </summary>
        /// <param name="provider">The provider this claims filter will operate against.</param>
        public FacebookClaimsFilter(string provider)
            : base(provider)
        {
        }

        /// <summary>
        /// Transforms the claims if this provider is used.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        protected override IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims)
        {
            var nameClaim = claims.FirstOrDefault(x => x.Type == "urn:facebook:name");
            if (nameClaim != null)
            {
                var list = claims.ToList();
                list.Remove(nameClaim);
                list.RemoveAll(x => x.Type == Constants.ClaimTypes.Name);
                list.Add(new Claim(Constants.ClaimTypes.Name, nameClaim.Value));
                return list;
            }
            return claims;
        }
    }
}
