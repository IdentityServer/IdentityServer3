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
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryUserService : IUserService
    {
        readonly List<InMemoryUser> _users = new List<InMemoryUser>();

        public InMemoryUserService(IEnumerable<InMemoryUser> users)
        {
            _users.AddRange(users);
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

        public virtual Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            var query =
                from u in _users
                where u.Username == username && u.Password == password
                select u;
            
            var user = query.SingleOrDefault();
            if (user != null)
            {
                return Task.FromResult(new AuthenticateResult(user.Subject, GetDisplayName(user)));
            }

            return Task.FromResult<AuthenticateResult>(null);
        }

        public virtual Task<ExternalAuthenticateResult> AuthenticateExternalAsync(Models.ExternalIdentity externalUser)
        {
            var query =
                from u in _users
                where
                    u.Provider == externalUser.Provider.Name &&
                    u.ProviderId == externalUser.ProviderId
                select u;
            
            var user = query.SingleOrDefault();
            if (user == null)
            {
                var name = externalUser.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
                if (name == null)
                {
                    return Task.FromResult<ExternalAuthenticateResult>(null);
                }
                
                user = new InMemoryUser
                {
                    Subject = Guid.NewGuid().ToString("N"),
                    Provider = externalUser.Provider.Name,
                    ProviderId = externalUser.ProviderId,
                    Username = name.Value,
                    Claims = externalUser.Claims
                };
                _users.Add(user);
            }

            return Task.FromResult(new ExternalAuthenticateResult(user.Provider, user.Subject, GetDisplayName(user)));
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


        public virtual Task<bool> IsActive(string subject)
        {
            var query =
                from u in _users
                where
                    u.Subject == subject
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