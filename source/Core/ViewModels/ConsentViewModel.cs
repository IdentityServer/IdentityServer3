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
    /// Models the data needed to render the consent page.
    /// </summary>
    public class ConsentViewModel : ErrorViewModel
    {
        /// <summary>
        /// The URL to allow a user to login as a different user.
        /// </summary>
        /// <value>
        /// The login with different account URL.
        /// </value>
        public string LoginWithDifferentAccountUrl { get; set; }

        /// <summary>
        /// The URL to POST the user's consent. <see cref="UserConsent"/> for the model for the submitted data.
        /// </summary>
        /// <value>
        /// The consent URL.
        /// </value>
        public string ConsentUrl { get; set; }

        /// <summary>
        /// The anti forgery values.
        /// </summary>
        /// <value>
        /// The anti forgery.
        /// </value>
        public AntiForgeryTokenViewModel AntiForgery { get; set; }

        /// <summary>
        /// The display name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }

        /// <summary>
        /// The URL for more information about the client.
        /// </summary>
        /// <value>
        /// The client URL.
        /// </value>
        public string ClientUrl { get; set; }

        /// <summary>
        /// The URL for the client's logo image.
        /// </summary>
        /// <value>
        /// The client logo URL.
        /// </value>
        public string ClientLogoUrl { get; set; }

        /// <summary>
        /// Indicates if the "allow remember consent" is disabled and should not be displayed to the user.
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow remember consent]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRememberConsent { get; set; }

        /// <summary>
        /// Value to populate the "remember my choice" checkbox.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remember consent]; otherwise, <c>false</c>.
        /// </value>
        public bool RememberConsent { get; set; }

        /// <summary>
        /// List of identity scopes being requested.
        /// </summary>
        /// <value>
        /// The identity scopes.
        /// </value>
        public IEnumerable<ConsentScopeViewModel> IdentityScopes { get; set; }

        /// <summary>
        /// List of resource scopes being requested.
        /// </summary>
        /// <value>
        /// The resource scopes.
        /// </value>
        public IEnumerable<ConsentScopeViewModel> ResourceScopes { get; set; }
    }
}
