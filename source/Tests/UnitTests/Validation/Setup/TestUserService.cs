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
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Tests.Validation
{
    class TestUserService : IUserService
    {
        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
        {
            if (username == password)
            {
                var p = IdentityServerPrincipal.Create(username, username, "password", "idsvr");
                return Task.FromResult(new AuthenticateResult(p));
            }

            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal sub, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity user, SignInMessage message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            return Task.FromResult(true);
        }

        public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            throw new NotImplementedException();
        }
        
        public Task SignOutAsync(ClaimsPrincipal subject)
        {
            throw new NotImplementedException();
        }
    }
}