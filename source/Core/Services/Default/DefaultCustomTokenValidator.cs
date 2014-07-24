/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DefaultCustomTokenValidator : ICustomTokenValidator
    {
        public Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result)
        {
            return Task.FromResult(result);
        }

        public Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result)
        {
            throw new NotImplementedException();
        }
    }
}