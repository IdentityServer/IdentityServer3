/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DefaultCustomGrantValidator : ICustomGrantValidator
    {
        public Task<ClaimsPrincipal> ValidateAsync(ValidatedTokenRequest request)
        {
            return Task.FromResult<ClaimsPrincipal>(null);
        }
    }
}