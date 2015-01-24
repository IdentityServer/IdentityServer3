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

using Owin;
using System;
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    /// <summary>
    /// Configures the login and logout views and behavior.
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        public AuthenticationOptions()
        {
            EnableLocalLogin = true;
            EnableLoginHint = true;
            EnableSignOutPrompt = true;
            CookieOptions = new CookieOptions();
        }

        /// <summary>
        /// Gets or sets a value indicating whether local login is enabled.
        /// Disabling this setting will not display the username/password form on the login page. This also will disable the resource owner password flow.
        /// Defaults to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if local login is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableLocalLogin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the login_hint parameter is used to prepopulate the username field. Defaults to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if login_hint is used; otherwise, <c>false</c>.
        /// </value>
        public bool EnableLoginHint { get; set; }

        /// <summary>
        /// Gets or sets the cookie options.
        /// </summary>
        /// <value>
        /// The cookie options.
        /// </value>
        public CookieOptions CookieOptions { get; set; }

        /// <summary>
        /// Gets or sets the login page links.
        /// LoginPageLinks allow the login view to provide the user custom links to other web pages that they might need to visit before they can login (such as a registration page, or a password reset page).
        /// </summary>
        /// <value>
        /// The login page links.
        /// </value>
        public IEnumerable<LoginPageLink> LoginPageLinks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IdentityServer will show a confirmation page for sign-out.
        /// When a client initiates a sign-out, by default IdentityServer will ask the user for confirmation. This is a mitigation technique against "logout spam".
        /// Defaults to true.
        /// </summary>
        /// <value>
        /// <c>true</c> if sign-out prompt is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSignOutPrompt { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether IdentityServer will remember the last username entered on the login page. Defaults to false.
        /// </summary>
        /// <value>
        /// <c>true</c> if the last username will be remembered; otherwise, <c>false</c>.
        /// </value>
        public bool RememberLastUsername { get; set; }

        /// <summary>
        /// Allows configuring additional identity providers
        /// </summary>
        /// <value>
        /// A callback function for configuring identity providers.
        /// </value>
        public Action<IAppBuilder, string> IdentityProviders { get; set; }
    }
}