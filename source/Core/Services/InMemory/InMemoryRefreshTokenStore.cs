/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryRefreshTokenStore : IRefreshTokenStore
    {
        readonly ConcurrentDictionary<string, RefreshToken> _repository = new ConcurrentDictionary<string, RefreshToken>();

        public Task StoreAsync(string key, RefreshToken value)
        {
            _repository[key] = value;

            return Task.FromResult<object>(null);
        }

        public Task<RefreshToken> GetAsync(string key)
        {
            RefreshToken code;
            if (_repository.TryRemove(key, out code))
            {
                return Task.FromResult(code);
            }

            return Task.FromResult<RefreshToken>(null);
        }

        public Task RemoveAsync(string key)
        {
            RefreshToken val;
            _repository.TryRemove(key, out val);

            return Task.FromResult<object>(null);
        }
    }
}
