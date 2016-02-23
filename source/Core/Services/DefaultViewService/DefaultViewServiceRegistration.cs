﻿/*
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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using System;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Registration for the default view service.
    /// </summary>
    public class DefaultViewServiceRegistration : DefaultViewServiceRegistration<DefaultViewService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        public DefaultViewServiceRegistration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DefaultViewServiceRegistration(DefaultViewServiceOptions options)
            : base(options)
        {
        }
    }
    
    /// <summary>
    /// Registration for a customer view service derived from the DefaultViewService.
    /// </summary>
    public class DefaultViewServiceRegistration<T> : Registration<IViewService, T>
        where T : DefaultViewService
    {
        const string InnerRegistrationName = "DefaultViewServiceRegistration.inner";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        public DefaultViewServiceRegistration()
            : this(new DefaultViewServiceOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceRegistration"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public DefaultViewServiceRegistration(DefaultViewServiceOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            AdditionalRegistrations.Add(new Registration<DefaultViewServiceOptions>(options));

            if (options.ViewLoader == null)
            {
                if (options.CustomViewDirectory.IsPresent())
                {
                    options.ViewLoader = new Registration<IViewLoader>(provider =>
                    {
                        return new FileSystemWithEmbeddedFallbackViewLoader(options.CustomViewDirectory);
                    });
                }
                else
                {
                    options.ViewLoader = new Registration<IViewLoader, FileSystemWithEmbeddedFallbackViewLoader>();
                }
            }

            if (options.CacheViews)
            {
                AdditionalRegistrations.Add(new Registration<IViewLoader>(options.ViewLoader, InnerRegistrationName));
                var cache = new ResourceCache();
                AdditionalRegistrations.Add(new Registration<IViewLoader>(
                    resolver => new CachingLoader(cache, resolver.Resolve<IViewLoader>(InnerRegistrationName))));
            }
            else
            {
                AdditionalRegistrations.Add(options.ViewLoader);
            }
        }
    }
}
