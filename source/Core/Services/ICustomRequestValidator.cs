/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ICustomRequestValidator
    {
        Task<ValidationResult> ValidateAuthorizeRequestAsync(ValidatedAuthorizeRequest request);
        Task<ValidationResult> ValidateTokenRequestAsync(ValidatedTokenRequest request);
    }
}