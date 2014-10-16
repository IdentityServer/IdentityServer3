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
    public class InMemoryTokenHandleStore : ITokenHandleStore
    {
        readonly ConcurrentDictionary<string, Token> _repository = new ConcurrentDictionary<string, Token>();

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