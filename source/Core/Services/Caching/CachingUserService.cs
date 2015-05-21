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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Caching
{
    /// <summary>
    /// <see cref="IUserService"/> decorator implementation that uses the provided <see cref="ICache{T}"/> for caching the user profile data.
    /// </summary>
    public class CachingUserService : IUserService
    {
        readonly IUserService inner;
        readonly ICache<IEnumerable<Claim>> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingUserService"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="IUserService"/>.</param>
        /// <param name="cache">The cache.</param>
        /// <exception cref="System.ArgumentNullException">
        /// inner
        /// or
        /// cache
        /// </exception>
        public CachingUserService(IUserService inner, ICache<IEnumerable<Claim>> cache)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cache == null) throw new ArgumentNullException("cache");

            this.inner = inner;
            this.cache = cache;
        }

        /// <summary>
        /// This method gets called before the login page is shown. This allows you to authenticate the
        /// user somehow based on data coming from the host (e.g. client certificates or trusted headers)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The authentication result or null to continue the flow.
        /// </returns>
        public Task<AuthenticateResult> PreAuthenticateAsync(PreAuthenticationContext context)
        {
            return inner.PreAuthenticateAsync(context);
        }

        /// <summary>
        /// This method gets called for local authentication (whenever the user uses the username and password dialog).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The authentication result.
        /// </returns>
        public Task<AuthenticateResult> AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            return inner.AuthenticateLocalAsync(context);
        }

        /// <summary>
        /// This method gets called when the user uses an external identity provider to authenticate.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The authentication result.
        /// </returns>
        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            return inner.AuthenticateExternalAsync(context);
        }

        /// <summary>
        /// This method gets called when the user signs out.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task SignOutAsync(SignOutContext context)
        {
            return inner.SignOutAsync(context);
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="requestedClaimTypes">The requested claim types. The user service is expected to filter based
        /// upon the requested claim types. <c>null</c> is passed if there is no filtering to be performed.</param>
        /// <returns>
        /// Claims for the subject
        /// </returns>
        public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            var key = GetKey(subject, requestedClaimTypes);
            return cache.GetAsync(key, ()=>inner.GetProfileDataAsync(subject, requestedClaimTypes));
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>
        ///   <c>true</c> if the user is still allowed to receive tokens; <c>false</c> otherwise.
        /// </returns>
        public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            return inner.IsActiveAsync(subject);
        }

        private string GetKey(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes)
        {
            var sub = subject.GetSubjectId();
            if (requestedClaimTypes == null) return sub;

            return sub + ":" + requestedClaimTypes.OrderBy(x => x).Aggregate((x, y) => x + "," + y);
        }
    }
}
