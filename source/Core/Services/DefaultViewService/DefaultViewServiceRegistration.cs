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
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class DefaultViewServiceRegistration : Registration<IViewService>
    {
        public DefaultViewServiceRegistration()
        {
            this.TypeFactory = () => new DefaultViewService();
        }

        public DefaultViewServiceRegistration(DefaultViewServiceConfiguration config)
        {
            this.TypeFactory = () => new DefaultViewService(config);
        }
    }

    public class DefaultViewServiceConfiguration
    {
        internal static readonly DefaultViewServiceConfiguration Default = 
            new DefaultViewServiceConfiguration();

        public DefaultViewServiceConfiguration()
        {
            // adding default CSS here so hosting application can choose to remove it
            Stylesheets = new HashSet<string>
            {
                "~/assets/styles.min.css"
            };

            
            Scripts = new HashSet<string>();
            CacheViews = true;
        }

        static volatile IViewLoader _loader = null;
        internal IViewLoader GetLoader()
        {
            if (_loader == null)
            {
                IViewLoader loader = ViewLoader ?? new FileSystemWithEmbeddedFallbackViewLoader();
                if (CacheViews)
                {
                    loader = new CachingLoader(loader);
                }
                _loader = loader;
            }
            return _loader;
        }
        
        public ICollection<string> Stylesheets { get; set; }
        public ICollection<string> Scripts { get; set; }
        public IViewLoader ViewLoader { get; set; }
        public bool CacheViews { get; set; }
    }
}
