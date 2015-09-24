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
using System.Security.Claims;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Models a mechanism for claims transformation for claims provided from external identity providers.
    /// </summary>
    public interface IExternalClaimsFilter
    {
        /// <summary>
        /// Filters the specified claims from an external identity provider.
        /// </summary>
        /// <param name="provider">The identifier for the external identity provider.</param>
        /// <param name="claims">The incoming claims.</param>
        /// <returns>The transformed claims.</returns>
        IEnumerable<Claim> Filter(string provider, IEnumerable<Claim> claims);
    }
}