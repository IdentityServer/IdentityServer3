/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly ConcurrentDictionary<string, AuthorizationCode> _repository = new ConcurrentDictionary<string, AuthorizationCode>();

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