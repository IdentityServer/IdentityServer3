/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ICustomTokenValidator
    {
        Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result);
        Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result);
    }
}