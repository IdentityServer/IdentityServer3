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
    /// Implementation of claims filter that filters out the claim types indicated.
    /// </summary>
    public class IgnoreClaimsFilter : IExternalClaimsFilter
    {
        readonly string[] claimTypesToIgnore;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreClaimsFilter"/> class.
        /// </summary>
        /// <param name="claimTypesToIgnore">The claim types to ignore.</param>
        public IgnoreClaimsFilter(params string[] claimTypesToIgnore)
        {
            this.claimTypesToIgnore = claimTypesToIgnore;
        }

        /// <summary>
        /// Filters the specified claims from an external identity provider.
        /// </summary>
        /// <param name="provider">The identifier for the external identity provider.</param>
        /// <param name="claims">The incoming claims.</param>
        /// <returns>
        /// The transformed claims.
        /// </returns>
        public IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims)
        {
            return claims.Where(x => !claimTypesToIgnore.Contains(x.Type));
        }
    }
}
