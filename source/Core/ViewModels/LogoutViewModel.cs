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

namespace Thinktecture.IdentityServer.Core.ViewModels
{
    /// <summary>
    /// Models the data needed to render the logout prompt page.
    /// </summary>
    public class LogoutViewModel : CommonViewModel
    {
        /// <summary>
        /// The URL to POST to to trigger a logout.
        /// </summary>
        /// <value>
        /// The logout URL.
        /// </value>
        public string LogoutUrl { get; set; }
        
        /// <summary>
        /// The anti forgery values.
        /// </summary>
        /// <value>
        /// The anti forgery.
        /// </value>
        public AntiForgeryHiddenInputViewModel AntiForgery { get; set; }
        
        /// <summary>
        /// The name of the client that requested the logout.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }
    }
}
