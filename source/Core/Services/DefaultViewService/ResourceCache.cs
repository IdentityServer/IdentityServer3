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

#pragma warning disable 1591

using System.Collections.Concurrent;
using System.ComponentModel;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// In-memory cache used by the view service
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ResourceCache
    {
        readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

        public string Read(string name)
        {
            string value;
            if (cache.TryGetValue(name, out value))
            {
                return value;
            }
            return null;
        }

        public void Write(string name, string value)
        {
            cache[name] = value;
        }
    }
}
