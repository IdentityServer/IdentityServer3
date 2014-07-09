/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ITransientDataRepository<T>
    {
        Task StoreAsync(string key, T value);
        Task<T> GetAsync(string key);
        Task RemoveAsync(string key);
    }
}
