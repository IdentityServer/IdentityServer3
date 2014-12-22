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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.Caching
{
    public class TimeoutCache<T> : ICache<T>
    {
        public TimeSpan Duration { get; set; }
        ICache<T> inner;
        InMemoryCache<DateTime> issued;

        protected virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public TimeoutCache(TimeSpan duration, ICache<T> inner)
        {
            if (inner == null) throw new ArgumentNullException("inner");

            this.Duration = duration;
            this.inner = inner;
            this.issued = new InMemoryCache<DateTime>();
        }

        public bool TryGet(string key, out T item)
        {
            item = default(T);

            DateTime itemIssued;
            if (!this.issued.TryGet(key, out itemIssued))
            {
                return false;
            }
            
            if (itemIssued < UtcNow.Subtract(Duration))
            {
                return false;
            }

            return this.inner.TryGet(key, out item);
        }

        public void Set(string key, T item)
        {
            issued.Set(key, UtcNow);
            inner.Set(key, item);
        }
    }
}
