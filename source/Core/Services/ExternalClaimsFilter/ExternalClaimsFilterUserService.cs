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
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class ExternalClaimsFilterUserService : IUserService
    {
        IExternalClaimsFilter filter;
        IUserService inner;

        public ExternalClaimsFilterUserService(IExternalClaimsFilter filter, IUserService inner)
        {
            this.filter = filter;
            this.inner = inner;
        }

        public System.Threading.Tasks.Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message, IDictionary<string, object> env)
        {
            return inner.PreAuthenticateAsync(message, env);
        }

        public System.Threading.Tasks.Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message = null, IDictionary<string, object> env = null)
        {
            return inner.AuthenticateLocalAsync(username, password, message, env);
        }

        public System.Threading.Tasks.Task<AuthenticateResult> AuthenticateExternalAsync(Models.ExternalIdentity externalUser, IDictionary<string, object> env)
        {
            externalUser.Claims = filter.Filter(externalUser.Provider, externalUser.Claims);
            return inner.AuthenticateExternalAsync(externalUser, env);
        }

        public System.Threading.Tasks.Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            return inner.GetProfileDataAsync(subject, requestedClaimTypes);
        }

        public System.Threading.Tasks.Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            return inner.IsActiveAsync(subject);
        }

        public System.Threading.Tasks.Task SignOutAsync(ClaimsPrincipal subject, IDictionary<string, object> env)
        {
            return inner.SignOutAsync(subject, env);
        }
    }
}
