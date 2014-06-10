/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryTokenHandleStore : ITokenHandleStore
    {
        ConcurrentDictionary<string, Token> _repository = new ConcurrentDictionary<string, Token>();

        public Task StoreAsync(string key, Token value)
        {
            _repository[key] = value;

            return Task.FromResult<object>(null);
        }

        public Task<Token> GetAsync(string key)
        {
            Token token;
            if (_repository.TryGetValue(key, out token))
            {
                return Task.FromResult(token);
            }

            return Task.FromResult<Token>(null);
        }

        public Task RemoveAsync(string key)
        {
            Token token;
            _repository.TryRemove(key, out token);

            return Task.FromResult<object>(null);
        }
    }
}