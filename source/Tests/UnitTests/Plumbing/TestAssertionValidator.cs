/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestAssertionValidator : ICustomGrantValidator
    {
        public Task<ClaimsPrincipal> ValidateAsync(ValidatedTokenRequest request)
        {
            if (request.GrantType == "customGrant")
            {
                return Task.FromResult(Principal.Create("CustomGrant", new Claim("sub", "bob")));
            };

            return Task.FromResult<ClaimsPrincipal>(null);
        }
    }
}