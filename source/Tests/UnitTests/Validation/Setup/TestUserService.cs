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

using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System.Threading.Tasks;

namespace IdentityServer3.Tests.Validation
{
    class TestUserService : IUserService
    {
        public Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            if (context.UserName == context.Password)
            {
                var p = IdentityServerPrincipal.Create(context.UserName, context.UserName, "password", "idsvr");
                context.AuthenticateResult = new AuthenticateResult(p);
            }
            else
            {
                context.AuthenticateResult = new AuthenticateResult("Username and/or password incorrect");
            }

            return Task.FromResult(0);
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return Task.FromResult(0);
        }

        public Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            return Task.FromResult(0);
        }

        public Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(0);
        }

        public Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            return Task.FromResult(0);
        }
        
        public Task SignOutAsync(SignOutContext context)
        {
            return Task.FromResult(0);
        }
    }
}