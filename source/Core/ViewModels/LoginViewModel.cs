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
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.ViewModels
{
    /// <summary>
    /// Models that data needed to render the login page.
    /// </summary>
    public class LoginViewModel : ErrorViewModel
    {
        /// <summary>
        /// The URL to POST credentials to for local logins. Will be <c>null</c> if local login is disabled.
        /// <see cref="LoginCredentials"/> for the model for the submitted data.
        /// </summary>
        /// <value>
        /// The login URL.
        /// </value>
        public string LoginUrl { get; set; }

        /// <summary>
        /// The anti forgery values.
        /// </summary>
        /// <value>
        /// The anti forgery.
        /// </value>
        public AntiForgeryTokenViewModel AntiForgery { get; set; }

        /// <summary>
        /// Indicates if "remember me" has been disabled and should not be displayed to the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow remember me]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRememberMe { get; set; }

        /// <summary>
        /// The value to populate the "remember me" field.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remember me]; otherwise, <c>false</c>.
        /// </value>
        public bool RememberMe { get; set; }

        /// <summary>
        /// The value to populate the username field.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username { get; set; }

        /// <summary>
        /// List of external providers to display for home realm discover (HRD). 
        /// </summary>
        /// <value>
        /// The external providers.
        /// </value>
        public IEnumerable<LoginPageLink> ExternalProviders { get; set; }

        /// <summary>
        /// List of additional links configured to be displayed on the login page (e.g. as registration, or forgot password links).
        /// </summary>
        /// <value>
        /// The additional links.
        /// </value>
        public IEnumerable<LoginPageLink> AdditionalLinks { get; set; }
    }
}
