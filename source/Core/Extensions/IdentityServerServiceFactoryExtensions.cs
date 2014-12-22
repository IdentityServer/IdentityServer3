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
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Caching;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class IdentityServerServiceFactoryExtensions
    {
        public static void ConfigureScopeStoreCache(this IdentityServerServiceFactory factory, Registration<ICache<IEnumerable<Scope>>> cacheRegistration = null)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (factory.ScopeStore == null) throw new ArgumentNullException("ScopeStore needs to be configured on the factory");

            if (cacheRegistration == null)
            {
                var timeoutCache = new TimeoutCache<IEnumerable<Scope>>(Constants.DefaultCacheDuration, new InMemoryCache<IEnumerable<Scope>>());
                var reg = new Registration<ICache<IEnumerable<Scope>>>(timeoutCache, "cache");
                factory.Register(reg);
            }
            else
            {
                factory.Register(new Registration<ICache<IEnumerable<Scope>>>(cacheRegistration, "cache"));
            }

            factory.Register(new Registration<IScopeStore>(factory.ScopeStore, "inner"));
            factory.ScopeStore = new Registration<IScopeStore>(resolver =>
            {
                var inner = resolver.Resolve<IScopeStore>("inner");
                var cache = resolver.Resolve<ICache<IEnumerable<Scope>>>("cache");
                return new CachingScopeStore(inner, cache);
            });
        }

        public static void ConfigureClientStoreCache(this IdentityServerServiceFactory factory, Registration<ICache<Client>> cacheRegistration = null)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (factory.ClientStore == null) throw new ArgumentNullException("ClientStore needs to be configured on the factory");

            if (cacheRegistration == null)
            {
                var timeoutCache = new TimeoutCache<Client>(Constants.DefaultCacheDuration, new InMemoryCache<Client>());
                var reg = new Registration<ICache<Client>>(timeoutCache, "cache");
                factory.Register(reg);
            }
            else
            {
                factory.Register(new Registration<ICache<Client>>(cacheRegistration, "cache"));
            }

            factory.Register(new Registration<IClientStore>(factory.ClientStore, "inner"));
            factory.ClientStore = new Registration<IClientStore>(resolver =>
            {
                var inner = resolver.Resolve<IClientStore>("inner");
                var cache = resolver.Resolve<ICache<Client>>("cache");
                return new CachingClientStore(inner, cache);
            });
        }
    }

   
}
