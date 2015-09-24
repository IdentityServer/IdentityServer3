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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Caching
{
    /// <summary>
    /// <see cref="IScopeStore"/> decorator implementation that uses the provided <see cref="ICache{T}"/> for caching the scopes.
    /// </summary>
    public class CachingScopeStore : IScopeStore
    {
        const string AllScopes = "CachingScopeStore.allscopes";
        const string AllScopesPublic = AllScopes + ".public";

        readonly IScopeStore inner;
        readonly ICache<IEnumerable<Scope>> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingScopeStore"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="IScopeStore"/>.</param>
        /// <param name="cache">The cache.</param>
        /// <exception cref="System.ArgumentNullException">
        /// inner
        /// or
        /// cache
        /// </exception>
        public CachingScopeStore(IScopeStore inner, ICache<IEnumerable<Scope>> cache)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cache == null) throw new ArgumentNullException("cache");

            this.inner = inner;
            this.cache = cache;
        }

        /// <summary>
        /// Gets all scopes.
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns>
        /// List of scopes
        /// </returns>
        public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            var key = GetKey(scopeNames);
            return await cache.GetAsync(key, async () => await inner.FindScopesAsync(scopeNames));
        }

        /// <summary>
        /// Gets all defined scopes.
        /// </summary>
        /// <param name="publicOnly">if set to <c>true</c> only public scopes are returned.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var key = GetKey(publicOnly);
            return await cache.GetAsync(key, async () => await inner.GetScopesAsync(publicOnly));
        }

        private string GetKey(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null || !scopeNames.Any()) return "";
            return scopeNames.OrderBy(x => x).Aggregate((x, y) => x + "," + y);
        }

        private string GetKey(bool publicOnly)
        {
            if (publicOnly) return AllScopesPublic;
            return AllScopes;
        }
    }
}
