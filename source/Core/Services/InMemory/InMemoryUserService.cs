/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryUserService : IUserService
    {
        List<InMemoryUser> _users = new List<InMemoryUser>();

        public InMemoryUserService(IEnumerable<InMemoryUser> users)
        {
            this._users.AddRange(users);
        }

        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            var query =
                from u in _users
                where u.Username == username && u.Password == password
                select u;
            
            var user = query.SingleOrDefault();
            if (user != null)
            {
                return Task.FromResult(new AuthenticateResult(user.Subject, user.Username));
            }

            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, Models.ExternalIdentity externalUser)
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
                var claims = externalUser.Claims.Except(new Claim[] { name });

                user = new InMemoryUser
                {
                    Subject = Guid.NewGuid().ToString("N"),
                    Provider = externalUser.Provider.Name,
                    ProviderId = externalUser.ProviderId,
                    Username = name.Value,
                    Claims = claims.ToArray()
                };
                _users.Add(user);
            }

            return Task.FromResult(new ExternalAuthenticateResult(user.Provider, user.Subject, user.Username));
        }


        public Task<IEnumerable<Claim>> GetProfileDataAsync(string subject, IEnumerable<string> requestedClaimTypes = null)
        {
            var query =
                from u in _users
                where u.Subject == subject
                select u;
            var user = query.Single();

            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, user.Subject)
            };

            claims.AddRange(user.Claims);
            if (requestedClaimTypes != null)
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type)).ToList();
            }

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }


        public Task<bool> IsActive(string subject)
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