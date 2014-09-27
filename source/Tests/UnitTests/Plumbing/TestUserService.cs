/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestUserService : IUserService
    {
        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password)
        {
            if (username == password)
            {

                var p = IdentityServerPrincipal.Create(username, username, "password", "idsvr");
                return Task.FromResult(new AuthenticateResult(p));
            }

            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<IEnumerable<System.Security.Claims.Claim>> GetProfileDataAsync(ClaimsPrincipal sub, IEnumerable<string> requestedClaimTypes = null)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateExternalAsync(Thinktecture.IdentityServer.Core.Models.ExternalIdentity user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsActive(string subject)
        {
            throw new NotImplementedException();
        }
    }
}