/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestAssertionValidator : IAssertionGrantValidator
    {
        public Task<ClaimsPrincipal> ValidateAsync(ValidatedTokenRequest request, IUserService users)
        {
            if (request.GrantType == "assertionType" && request.Assertion == "assertion")
            {
                return Task.FromResult(Principal.Create("Assertion", new Claim("sub", "bob")));
            };

            return Task.FromResult<ClaimsPrincipal>(null);
        }
    }
}