/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ICustomRequestValidator
    {
        Task<ValidationResult> ValidateAuthorizeRequestAsync(ValidatedAuthorizeRequest request, IUserService profile);
        Task<ValidationResult> ValidateTokenRequestAsync(ValidatedTokenRequest request, IUserService profile);
    }
}