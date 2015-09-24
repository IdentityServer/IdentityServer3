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

using IdentityModel;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.InMemory
{
    /// <summary>
    /// In-memory user service
    /// </summary>
    public class InMemoryUserService : UserServiceBase
    {
        readonly List<InMemoryUser> _users;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryUserService"/> class.
        /// </summary>
        /// <param name="users">The users.</param>
        public InMemoryUserService(List<InMemoryUser> users)
        {
            _users = users;
        }

        /// <summary>
        /// This methods gets called for local authentication (whenever the user uses the username and password dialog).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var query =
                from u in _users
                where u.Username == context.UserName && u.Password == context.Password
                select u;

            var user = query.SingleOrDefault();
            if (user != null)
            {
                var p = IdentityServerPrincipal.Create(user.Subject, GetDisplayName(user), Constants.AuthenticationMethods.Password, Constants.BuiltInIdentityProvider);
                context.AuthenticateResult = new AuthenticateResult(p);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// This method gets called when the user uses an external identity provider to authenticate.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            var query =
                from u in _users
                where
                    u.Provider == context.ExternalIdentity.Provider &&
                    u.ProviderId == context.ExternalIdentity.ProviderId
                select u;

            var user = query.SingleOrDefault();
            if (user == null)
            {
                string displayName;

                var name = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
                if (name == null)
                {
                    displayName = context.ExternalIdentity.ProviderId;
                }
                else
                {
                    displayName = name.Value;
                }

                user = new InMemoryUser
                {
                    Subject = CryptoRandom.CreateUniqueId(),
                    Provider = context.ExternalIdentity.Provider,
                    ProviderId = context.ExternalIdentity.ProviderId,
                    Username = displayName,
                    Claims = context.ExternalIdentity.Claims
                };
                _users.Add(user);
            }

            var p = IdentityServerPrincipal.Create(user.Subject, GetDisplayName(user), Constants.AuthenticationMethods.External, user.Provider);
            context.AuthenticateResult = new AuthenticateResult(p);
            
            return Task.FromResult(0);
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var query =
                from u in _users
                where u.Subject == context.Subject.GetSubjectId()
                select u;
            var user = query.Single();

            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, user.Subject),
            };

            claims.AddRange(user.Claims);
            if (!context.AllClaimsRequested)
            {
                claims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
            }

            context.IssuedClaims = claims;

            return Task.FromResult(0);
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. during token issuance or validation)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">subject</exception>
        public override Task IsActiveAsync(IsActiveContext context)
        {
            if (context.Subject == null) throw new ArgumentNullException("subject");

            var query =
                from u in _users
                where
                    u.Subject == context.Subject.GetSubjectId()
                select u;

            var user = query.SingleOrDefault();
            
            context.IsActive = (user != null) && user.Enabled;

            return Task.FromResult(0);
        }

        /// <summary>
        /// Retrieves the display name.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual string GetDisplayName(InMemoryUser user)
        {
            var nameClaim = user.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
            if (nameClaim != null)
            {
                return nameClaim.Value;
            }

            return user.Username;
        }
    }
}