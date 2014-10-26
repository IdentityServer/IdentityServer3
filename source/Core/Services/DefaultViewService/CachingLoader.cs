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

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class CachingLoader : IViewLoader
    {
        readonly ResourceCache cache = new ResourceCache();
        readonly IViewLoader inner;

        public CachingLoader(IViewLoader inner)
        {
            this.inner = inner;
        }

        public string Load(string name)
        {
            var value = cache.Read(name);
            if (value == null)
            {
                value = inner.Load(name);
                cache.Write(name, value);
            }
            return value;
        }
    }
}
