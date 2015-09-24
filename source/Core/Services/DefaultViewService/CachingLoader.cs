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

using System.Threading.Tasks;
namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// <see cref="IViewLoader"/> decorator implementation that caches HTML templates in-memory.
    /// </summary>
    public class CachingLoader : IViewLoader
    {
        readonly ResourceCache cache;
        readonly IViewLoader inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingLoader" /> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="inner">The inner.</param>
        public CachingLoader(ResourceCache cache, IViewLoader inner)
        {
            this.cache = cache;
            this.inner = inner;
        }

        /// <summary>
        /// Loads the HTML for the named view.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public async Task<string> LoadAsync(string name)
        {
            var value = cache.Read(name);
            if (value == null)
            {
                value = await inner.LoadAsync(name);
                cache.Write(name, value);
            }
            return value;
        }
    }
}
