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

using System.Collections.Generic;

namespace IdentityServer3.Core.ViewModels
{
    /// <summary>
    /// Models the data needed to render the logged out page.
    /// </summary>
    public class LoggedOutViewModel : CommonViewModel
    {
        /// <summary>
        /// A list of URLs that must be displayed in hidden iframes in the rendered page. These are
        /// needed to trigger logout of other endpoints.
        /// </summary>
        /// <value>
        /// The iframe urls.
        /// </value>
        public IEnumerable<string> IFrameUrls { get; set; }

        /// <summary>
        /// The name of the client that requested the logout.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }
        
        /// <summary>
        /// The URL to allow the user to return the the <see cref="ClientName"/>.
        /// </summary>
        /// <value>
        /// The redirect URL.
        /// </value>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic redirect to the redirect URL is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if automatic redirect is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRedirect { get; set; }
        
        /// <summary>
        /// Gets or sets the automatic redirect delay (in seconds).
        /// </summary>
        /// <value>
        /// The automatic redirect delay.
        /// </value>
        public int AutoRedirectDelay { get; set; }
    }
}
