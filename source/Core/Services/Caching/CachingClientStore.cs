/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using System;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Caching
{
    /// <summary>
    /// <see cref="IClientStore"/> decorator implementation that uses the provided <see cref="ICache{T}"/> for caching clients.
    /// </summary>
    public class CachingClientStore : IClientStore
    {
        readonly IClientStore inner;
        readonly ICache<Client> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingClientStore"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="IClientStore"/>.</param>
        /// <param name="cache">The cache.</param>
        /// <exception cref="System.ArgumentNullException">
        /// inner
        /// or
        /// cache
        /// </exception>
        public CachingClientStore(IClientStore inner, ICache<Client> cache)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cache == null) throw new ArgumentNullException("cache");

            this.inner = inner;
            this.cache = cache;
        }

        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            return await cache.GetAsync(clientId, async () => await inner.FindClientByIdAsync(clientId));
        }
    }
}
