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

namespace IdentityServer3.Core.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    /// The anonymous claims provider is responsible for determining which claims to include in tokens when a 
    /// request for a anon token is processed
    /// </summary>
    public interface IAnonymousClaimsProvider
    {
        /// <summary>
        /// Returns a default set of claims for an anonymous identity or access token 
        /// </summary>
        /// <returns></returns>
        IEnumerable<Claim> GetAnonymousClaims();
    }
}