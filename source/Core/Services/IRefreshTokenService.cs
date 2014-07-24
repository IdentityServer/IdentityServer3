/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IRefreshTokenService
    {
        Task<string> CreateRefreshTokenAsync(Token accessToken, Client client);
        Task<string> UpdateRefreshTokenAsync(RefreshToken refreshToken, Client client);
    }
}
