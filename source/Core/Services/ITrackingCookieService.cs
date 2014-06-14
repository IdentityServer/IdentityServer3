/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ITrackingCookieService
    {
        Task AddValueAsync(string name, string value);
        Task<IEnumerable<string>> GetValuesAndDeleteCookieAsync(string name);
    }
}