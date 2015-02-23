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
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    /// <summary>
    /// This interface allows IdentityServer to connect to your user and profile store.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// This method gets called before the login page is shown. This allows you to determine if the user should be authenticated by some out of band mechanism (e.g. client certificates or trusted headers).
        /// </summary>
        /// <param name="message">The signin message.</param>
        /// <returns>The authentication result or null to continue the flow.</returns>
        Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message);

        /// <summary>
        /// This method gets called for local authentication (whenever the user uses the username and password dialog).
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="message">The signin message.</param>
        /// <returns>The authentication result. </returns>
        Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message);

        /// <summary>
        /// This method gets called when the user uses an external identity provider to authenticate.
        /// The user's identity from the external provider is passed via the `externalUser` parameter which contains the 
        /// provider identifier, the provider's identifier for the user, and the claims from the provider for the external user.
        /// </summary>
        /// <param name="externalUser">The external user.</param>
        /// <param name="message">The signin message.</param>
        /// <returns>
        /// The authentication result.
        /// </returns>
        Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message);

        /// <summary>
        /// This method gets called when the user signs out.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns></returns>
        Task SignOutAsync(ClaimsPrincipal subject);

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="requestedClaimTypes">The requested claim types. The user service is expected to filter based 
        /// upon the requested claim types. <c>null</c> is passed if there is no filtering to be performed.</param>
        /// <returns>Claims for the subject</returns>
        Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null);

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns><c>true</c> if the user is still allowed to receive tokens; <c>false</c> otherwise.</returns>
        Task<bool> IsActiveAsync(ClaimsPrincipal subject);
    }
}