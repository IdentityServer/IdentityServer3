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

using IdentityServer3.Core.Configuration;
using System.Collections.Generic;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Configures the assets for the default view service.
    /// </summary>
    public class DefaultViewServiceOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewServiceOptions"/> class.
        /// </summary>
        public DefaultViewServiceOptions()
        {
            // adding default CSS here so hosting application can choose to remove it
            Stylesheets = new List<string>
            {
                "~/assets/styles.min.css"
            };
            
            Scripts = new List<string>();
            CacheViews = true;
        }

        /// <summary>
        /// Stylesheets to be rendered into the layout.
        /// </summary>
        /// <value>
        /// The stylesheets.
        /// </value>
        public IList<string> Stylesheets { get; set; }
        
        /// <summary>
        /// Scripts to be rendered into the layout.
        /// </summary>
        /// <value>
        /// The scripts.
        /// </value>
        public IList<string> Scripts { get; set; }

        /// <summary>
        /// Gets or sets the registration for the view loader.
        /// </summary>
        /// <value>
        /// The view loader.
        /// </value>
        public Registration<IViewLoader> ViewLoader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTML will be cached by the default view cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cache views]; otherwise, <c>false</c>.
        /// </value>
        public bool CacheViews { get; set; }
    }
}
