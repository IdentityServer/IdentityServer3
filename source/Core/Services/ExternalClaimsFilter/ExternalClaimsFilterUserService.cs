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

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.ExternalClaimsFilter
{
    internal class ExternalClaimsFilterUserService : IUserService
    {
        readonly IExternalClaimsFilter _filter;
        readonly IUserService _inner;

        public ExternalClaimsFilterUserService(IExternalClaimsFilter filter, IUserService inner)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            if (inner == null) throw new ArgumentNullException("inner");

            _filter = filter;
            _inner = inner;
        }

        public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            return _inner.PreAuthenticateAsync(message);
        }

        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message = null)
        {
            return _inner.AuthenticateLocalAsync(username, password, message);
        }

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
        {
            externalUser.Claims = _filter.Filter(externalUser.Provider, externalUser.Claims);
            return _inner.AuthenticateExternalAsync(externalUser, message);
        }

        public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            return _inner.GetProfileDataAsync(subject, requestedClaimTypes);
        }

        public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            return _inner.IsActiveAsync(subject);
        }

        public Task SignOutAsync(ClaimsPrincipal subject)
        {
            return _inner.SignOutAsync(subject);
        }
    }
}
