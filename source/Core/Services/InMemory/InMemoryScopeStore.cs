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

using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.InMemory
{
    /// <summary>
    /// In-memory scope store
    /// </summary>
    public class InMemoryScopeStore : IScopeStore
    {
        readonly IEnumerable<Scope> _scopes;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryScopeStore"/> class.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        public InMemoryScopeStore(IEnumerable<Scope> scopes)
        {
            _scopes = scopes;
        }

        /// <summary>
        /// Gets all scopes.
        /// </summary>
        /// <returns>
        /// List of scopes
        /// </returns>
        public Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException("scopeNames");
            
            var scopes = from s in _scopes
                         where scopeNames.ToList().Contains(s.Name)
                         select s;

            return Task.FromResult<IEnumerable<Scope>>(scopes.ToList());
        }


        /// <summary>
        /// Gets all defined scopes.
        /// </summary>
        /// <param name="publicOnly">if set to <c>true</c> only public scopes are returned.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            if (publicOnly)
            {
                return Task.FromResult(_scopes.Where(s => s.ShowInDiscoveryDocument));
            }

            return Task.FromResult(_scopes);
        }
    }
}