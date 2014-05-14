/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.WsFed.Services
{
    public interface ICookieService
    {
        Task AddValueAsync(string value);
        Task<IEnumerable<string>> GetValuesAndDeleteCookieAsync();
    }
}