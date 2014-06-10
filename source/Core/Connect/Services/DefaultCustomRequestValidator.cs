/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultCustomRequestValidator : ICustomRequestValidator
    {
        public Task<ValidationResult> ValidateAuthorizeRequestAsync(ValidatedAuthorizeRequest request, IUserService profile)
        {
            return Task.FromResult(new ValidationResult
            {
                IsError = false
            });
        }

        public Task<ValidationResult> ValidateTokenRequestAsync(ValidatedTokenRequest request, IUserService profile)
        {
            return Task.FromResult(new ValidationResult
            {
                IsError = false
            });
        }
    }
}