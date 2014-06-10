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
    public class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
    {
        ConcurrentDictionary<string, AuthorizationCode> _repository = new ConcurrentDictionary<string, AuthorizationCode>();

        public Task StoreAsync(string key, AuthorizationCode value)
        {
            _repository[key] = value;

            return Task.FromResult<object>(null);
        }

        public Task<AuthorizationCode> GetAsync(string key)
        {
            AuthorizationCode code;
            if (_repository.TryRemove(key, out code))
            {
                return Task.FromResult(code);
            }

            return Task.FromResult<AuthorizationCode>(null);
        }

        public Task RemoveAsync(string key)
        {
            AuthorizationCode val;
            _repository.TryRemove(key, out val);

            return Task.FromResult<object>(null);
        }
    }
}