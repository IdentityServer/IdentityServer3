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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Caching
{
    public class CachingScopeStore : IScopeStore
    {
        IScopeStore inner;
        ICache<IEnumerable<Scope>> cache;

        public CachingScopeStore(IScopeStore inner, ICache<IEnumerable<Scope>> cache)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cache == null) throw new ArgumentNullException("cache");

            this.inner = inner;
            this.cache = cache;
        }

        public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            var key = GetKey(scopeNames);
            return await cache.GetAsync(key, async () => await inner.FindScopesAsync(scopeNames));
        }

        public async Task<IEnumerable<Models.Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var key = GetKey(publicOnly);
            return await cache.GetAsync(key, async () => await inner.GetScopesAsync(publicOnly));
        }

        private string GetKey(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) return "";
            return scopeNames.OrderBy(x => x).Aggregate((x, y) => x + "," + y);
        }

        private string GetKey(bool publicOnly)
        {
            if (publicOnly) return "__all__.public";
            return "__all__";
        }
    }
}
