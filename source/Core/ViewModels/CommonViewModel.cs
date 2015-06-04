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

namespace IdentityServer3.Core.ViewModels
{
    /// <summary>
    /// Models common data needed to render pages in IdentityServer.
    /// </summary>
    public class CommonViewModel
    {
        /// <summary>
        /// The site URL.
        /// </summary>
        /// <value>
        /// The site URL.
        /// </value>
        public string SiteUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>
        /// The name of the site.
        /// </value>
        public string SiteName { get; set; }
        
        /// <summary>
        /// The current logged in display name.
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        public string CurrentUser { get; set; }

        /// <summary>
        /// The URL to allow a user to logout.
        /// </summary>
        /// <value>
        /// The logout URL.
        /// </value>
        public string LogoutUrl { get; set; }

        /// <summary>
        /// Gets or sets the custom data for the model.
        /// </summary>
        /// <value>
        /// The custom data.
        /// </value>
        public object Custom { get; set; }
    }
}
