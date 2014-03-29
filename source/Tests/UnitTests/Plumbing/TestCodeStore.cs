/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace UnitTests.Plumbing
{
    class TestCodeStore : IAuthorizationCodeStore
    {
        static ConcurrentDictionary<string, AuthorizationCode> _repository = 
            new ConcurrentDictionary<string, AuthorizationCode>();

        public async Task StoreAsync(string key, AuthorizationCode value)
        {
            _repository[key] = value;
        }

        public async Task<AuthorizationCode> GetAsync(string key)
        {
            AuthorizationCode code;
            if (_repository.TryRemove(key, out code))
            {
                return code;
            }

            return null;
        }

        public async Task RemoveAsync(string key)
        {
            AuthorizationCode val;
            _repository.TryRemove(key, out val);
        }
    }
}
