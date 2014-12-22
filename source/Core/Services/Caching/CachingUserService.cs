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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Services.Caching
{
    public class CachingUserService : IUserService
    {
        IUserService inner;
        ICache<IEnumerable<Claim>> cache;

        public CachingUserService(IUserService inner, ICache<IEnumerable<Claim>> cache)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cache == null) throw new ArgumentNullException("cache");

            this.inner = inner;
            this.cache = cache;
        }

        public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            return inner.PreAuthenticateAsync(message);
        }

        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message = null)
        {
            return inner.AuthenticateLocalAsync(username, password, message);
        }

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
        {
            return inner.AuthenticateExternalAsync(externalUser, message);
        }

        public Task SignOutAsync(ClaimsPrincipal subject)
        {
            return inner.SignOutAsync(subject);
        }

        public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            var key = GetKey(subject, requestedClaimTypes);
            return cache.GetAsync(key, ()=>inner.GetProfileDataAsync(subject, requestedClaimTypes));
        }

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
