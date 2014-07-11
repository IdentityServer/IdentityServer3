/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DefaultCustomRequestValidator : ICustomRequestValidator
    {
        public Task<ValidationResult> ValidateAuthorizeRequestAsync(ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(new ValidationResult
            {
                IsError = false
            });
        }

        public Task<ValidationResult> ValidateTokenRequestAsync(ValidatedTokenRequest request)
        {
            return Task.FromResult(new ValidationResult
            {
                IsError = false
            });
        }
    }
}