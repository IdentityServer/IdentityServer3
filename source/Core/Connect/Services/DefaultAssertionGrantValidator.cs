/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultAssertionGrantValidator : IAssertionGrantValidator
    {
        public Task<ClaimsPrincipal> ValidateAsync(ValidatedTokenRequest request, IUserService users)
        {
            return Task.FromResult<ClaimsPrincipal>(null);
        }
    }
}