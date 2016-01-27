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

using System.Threading.Tasks;
using System.Security.Claims;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default implementation of IAuthenticationSessionValidator. Always returns true.
    /// </summary>
    public class DefaultAuthenticationSessionValidator : IAuthenticationSessionValidator
    {
        /// <summary>
        /// This method is called whenever an authentication cookie is presented to IdentityServer for the logged in user.
        /// Return true to indicate the authentication cookie should be honored, false otherwise.
        /// </summary>
        /// <param name="subject">The user.</param>
        /// <returns>true if authentication session is valid, false otherwise.</returns>
        public Task<bool> IsAuthenticationSessionValidAsync(ClaimsPrincipal subject)
        {
            return Task.FromResult(true);
        }
    }
}
