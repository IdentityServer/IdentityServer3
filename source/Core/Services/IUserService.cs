/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    /// <summary>
    /// This interface connects identity server to your user and profile store
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// This methods gets called before the login page is shown. This allows you to authenticate the user somehow based on data coming from the host (e.g. client certificates or trusted headers)
        /// </summary>
        /// <param name="message">The signin message.</param>
        /// <returns>The authentication result or null to continue the flow</returns>
        Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message);

        /// <summary>
        /// This methods gets called for local authentication (whenever the user uses the username and password dialog).
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="message">The signin message.</param>
        /// <returns>The authentication result</returns>
        Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message = null);

        /// <summary>
        /// This method gets called when the user uses an external identity provider to authenticate.
        /// </summary>
        /// <param name="externalUser">The external user.</param>
        /// <returns>The authentication result.</returns>
        Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser);

        /// <summary>
        /// This method gets called when the user signs out (allows to cleanup resources)
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns></returns>
        Task SignOutAsync(ClaimsPrincipal subject);

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="requestedClaimTypes">The requested claim types.</param>
        /// <returns>Claims</returns>
        Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null);

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. during token issuance or validation)
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>true or false</returns>
        Task<bool> IsActiveAsync(ClaimsPrincipal subject);
    }
}