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
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryUserService : IUserService
    {
        readonly List<InMemoryUser> _users = new List<InMemoryUser>();

        public InMemoryUserService(IEnumerable<InMemoryUser> users)
        {
            _users.AddRange(users);
        }

        public virtual Task<AuthenticateResult> PreAuthenticateAsync(IDictionary<string, object> env, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public virtual Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
        {
            var query =
                from u in _users
                where u.Username == username && u.Password == password
                select u;

            var user = query.SingleOrDefault();
            if (user != null)
            {
                var p = IdentityServerPrincipal.Create(user.Subject, GetDisplayName(user), Constants.AuthenticationMethods.Password, Constants.BuiltInIdentityProvider);
                var result = new AuthenticateResult(p);
                return Task.FromResult(result);
            }

            return Task.FromResult<AuthenticateResult>(null);
        }

        public virtual Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser)
        {
            var query =
                from u in _users
                where
                    u.Provider == externalUser.Provider &&
                    u.ProviderId == externalUser.ProviderId
                select u;

            var user = query.SingleOrDefault();
            if (user == null)
            {
                var name = externalUser.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
                if (name == null)
                {
                    return Task.FromResult<AuthenticateResult>(null);
                }

                user = new InMemoryUser
                {
                    Subject = Guid.NewGuid().ToString("N"),
                    Provider = externalUser.Provider,
                    ProviderId = externalUser.ProviderId,
                    Username = name.Value,
                    Claims = externalUser.Claims
                };
                _users.Add(user);
            }

            var p = IdentityServerPrincipal.Create(user.Subject, GetDisplayName(user), Constants.AuthenticationMethods.External, user.Provider);
            var result = new AuthenticateResult(p);
            return Task.FromResult(result);
        }

        protected virtual string GetDisplayName(InMemoryUser user)
        {
            var nameClaim = user.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
            if (nameClaim != null)
            {
                return nameClaim.Value;
            }

            return user.Username;
        }

        public virtual Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            var query =
                from u in _users
                where u.Subject == subject.GetSubjectId()
                select u;
            var user = query.Single();

            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, user.Subject),
            };

            claims.AddRange(user.Claims);
            if (requestedClaimTypes != null)
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type)).ToList();
            }

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }

        public virtual Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");

            var query =
                from u in _users
                where
                    u.Subject == subject.GetSubjectId()
                select u;

            var user = query.SingleOrDefault();

            if (user == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(user.Enabled);
        }
    }
}