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

using IdentityServer3.Core.Models;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// This interface allows IdentityServer to connect to your user and profile store.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// This method gets called before the login page is shown. This allows you to determine if the user should be authenticated by some out of band mechanism (e.g. client certificates or trusted headers).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task PreAuthenticateAsync(PreAuthenticationContext context);

        /// <summary>
        /// This method gets called for local authentication (whenever the user uses the username and password dialog).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task AuthenticateLocalAsync(LocalAuthenticationContext context);

        /// <summary>
        /// This method gets called when the user uses an external identity provider to authenticate.
        /// The user's identity from the external provider is passed via the `externalUser` parameter which contains the
        /// provider identifier, the provider's identifier for the user, and the claims from the provider for the external user.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task AuthenticateExternalAsync(ExternalAuthenticationContext context);

        /// <summary>
        /// This method is called prior to the user being issued a login cookie for IdentityServer. 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task PostAuthenticateAsync(PostAuthenticationContext context);
        
        /// <summary>
        /// This method gets called when the user signs out.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task SignOutAsync(SignOutContext context);

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task GetProfileDataAsync(ProfileDataRequestContext context);

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task IsActiveAsync(IsActiveContext context);
    }
}