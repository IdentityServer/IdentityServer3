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
    /// <summary>
    /// Configures the default view service.
    /// </summary>
    public class DefaultViewServiceConfiguration
    {
        internal static readonly DefaultViewServiceConfiguration Default = 
            new DefaultViewServiceConfiguration();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceConfiguration"/> class.
        /// </summary>
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

        /// <summary>
        /// Stylesheets to be rendered into the layout.
        /// </summary>
        /// <value>
        /// The stylesheets.
        /// </value>
        public ICollection<string> Stylesheets { get; set; }
        
        /// <summary>
        /// Scripts to be rendered into the layout.
        /// </summary>
        /// <value>
        /// The scripts.
        /// </value>
        public ICollection<string> Scripts { get; set; }
        
        /// <summary>
        /// View loader used to load the HTML templates.
        /// </summary>
        /// <value>
        /// The view loader.
        /// </value>
        public IViewLoader ViewLoader { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether HTML will be cached by the default view cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cache views]; otherwise, <c>false</c>.
        /// </value>
        public bool CacheViews { get; set; }
    }
}
