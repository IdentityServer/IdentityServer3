/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IRefreshTokenStore : ITransientDataRepository<RefreshToken>
    {
    }
}