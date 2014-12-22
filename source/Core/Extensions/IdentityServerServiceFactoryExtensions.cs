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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Caching;
using Thinktecture.IdentityServer.Core.Services.Default;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class IdentityServerServiceFactoryExtensions
    {
        const string CachingRegistrationName = "IdentityServerServiceFactoryExtensions.cache";
        const string InnerRegistrationName = "IdentityServerServiceFactoryExtensions.inner";

        public static void ConfigureScopeStoreCache(this IdentityServerServiceFactory factory,
            Registration<ICache<IEnumerable<Scope>>> cacheRegistration)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (cacheRegistration == null) throw new ArgumentNullException("cacheRegistration");
            if (factory.ScopeStore == null) throw new ArgumentNullException("ScopeStore needs to be configured on the factory");

            factory.Register(new Registration<ICache<IEnumerable<Scope>>>(cacheRegistration, CachingRegistrationName));
            factory.Register(new Registration<IScopeStore>(factory.ScopeStore, InnerRegistrationName));

            factory.ScopeStore = new Registration<IScopeStore>(resolver =>
            {
                var inner = resolver.Resolve<IScopeStore>(InnerRegistrationName);
                var cache = resolver.Resolve<ICache<IEnumerable<Scope>>>(CachingRegistrationName);
                return new CachingScopeStore(inner, cache);
            });
        }

        public static void ConfigureScopeStoreCache(this IdentityServerServiceFactory factory, TimeSpan cacheDuration)
        {
            var cache = new DefaultCache<IEnumerable<Scope>>(cacheDuration);
            var cacheRegistration = new Registration<ICache<IEnumerable<Scope>>>(cache);
            factory.ConfigureScopeStoreCache(cacheRegistration);
        }
        
        public static void ConfigureScopeStoreCache(this IdentityServerServiceFactory factory)
        {
            var cache = new DefaultCache<IEnumerable<Scope>>();
            var cacheRegistration = new Registration<ICache<IEnumerable<Scope>>>(cache);
            factory.ConfigureScopeStoreCache(cacheRegistration);
        }

        public static void ConfigureClientStoreCache(this IdentityServerServiceFactory factory,
            Registration<ICache<Client>> cacheRegistration)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (cacheRegistration == null) throw new ArgumentNullException("cacheRegistration");
            if (factory.ClientStore == null) throw new ArgumentNullException("ClientStore needs to be configured on the factory");

            factory.Register(new Registration<ICache<Client>>(cacheRegistration, CachingRegistrationName));
            factory.Register(new Registration<IClientStore>(factory.ClientStore, InnerRegistrationName));
            
            factory.ClientStore = new Registration<IClientStore>(resolver =>
            {
                var inner = resolver.Resolve<IClientStore>(InnerRegistrationName);
                var cache = resolver.Resolve<ICache<Client>>(CachingRegistrationName);
                return new CachingClientStore(inner, cache);
            });
        }

        public static void ConfigureClientStoreCache(this IdentityServerServiceFactory factory, TimeSpan cacheDuration)
        {
            var cache = new DefaultCache<Client>(cacheDuration);
            var cacheRegistration = new Registration<ICache<Client>>(cache);
            factory.ConfigureClientStoreCache(cacheRegistration);
        }
        
        public static void ConfigureClientStoreCache(this IdentityServerServiceFactory factory)
        {
            var cache = new DefaultCache<Client>();
            var cacheRegistration = new Registration<ICache<Client>>(cache);
            factory.ConfigureClientStoreCache(cacheRegistration);
        }

        public static void ConfigureUserServiceCache(this IdentityServerServiceFactory factory,
           Registration<ICache<IEnumerable<Claim>>> cacheRegistration)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (cacheRegistration == null) throw new ArgumentNullException("cacheRegistration");
            if (factory.UserService == null) throw new ArgumentNullException("UserService needs to be configured on the factory");

            factory.Register(new Registration<ICache<IEnumerable<Claim>>>(cacheRegistration, CachingRegistrationName));
            factory.Register(new Registration<IUserService>(factory.UserService, InnerRegistrationName));

            factory.UserService = new Registration<IUserService>(resolver =>
            {
                var inner = resolver.Resolve<IUserService>(InnerRegistrationName);
                var cache = resolver.Resolve<ICache<IEnumerable<Claim>>>(CachingRegistrationName);
                return new CachingUserService(inner, cache);
            });
        }

        public static void ConfigureUserServiceCache(this IdentityServerServiceFactory factory, TimeSpan cacheDuration)
        {
            var cache = new DefaultCache<IEnumerable<Claim>>(cacheDuration);
            var cacheRegistration = new Registration<ICache<IEnumerable<Claim>>>(cache);
            factory.ConfigureUserServiceCache(cacheRegistration);
        }

        public static void ConfigureUserServiceCache(this IdentityServerServiceFactory factory)
        {
            var cache = new DefaultCache<IEnumerable<Claim>>();
            var cacheRegistration = new Registration<ICache<IEnumerable<Claim>>>(cache);
            factory.ConfigureUserServiceCache(cacheRegistration);
        }
    }
}
