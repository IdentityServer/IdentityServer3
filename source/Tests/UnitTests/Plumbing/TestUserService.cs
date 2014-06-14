/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestUserService : IUserService
    {
        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            if (username == password)
            {
                return Task.FromResult(new AuthenticateResult(username, username));
            }

            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(string sub, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }

        public Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, Thinktecture.IdentityServer.Core.Models.ExternalIdentity user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsActive(string subject)
        {
            throw new NotImplementedException();
        }
    }
}