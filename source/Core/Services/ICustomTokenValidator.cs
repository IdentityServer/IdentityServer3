/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ICustomTokenValidator
    {
        Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result, CoreSettings settings, IClientService clients, IUserService users);
        Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result, CoreSettings settings, IClientService clients, IUserService users);
    }
}