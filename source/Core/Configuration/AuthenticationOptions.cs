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

namespace IdentityServer3.Core.Configuration
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
            EnablePostSignOutAutoRedirect = false;
            PostSignOutAutoRedirectDelay = 0;
            RequireAuthenticatedUserForSignOutMessage = false;
            CookieOptions = new CookieOptions();
            SignInMessageThreshold = Constants.SignInMessageThreshold;
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
        /// Gets or sets a value indicating whether IdentityServer automatically redirects back to a validated post_logout_redirect_uri passed to the signout endpoint. Defaults to false.
        /// </summary>
        /// <value>
        /// <c>true</c> if automatic redirect after signout is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePostSignOutAutoRedirect { get; set; }

        /// <summary>
        /// Gets or sets the delay (in seconds) before redirecting to a post_logout_redirect_uri. Defaults to 0.
        /// </summary>
        /// <value>
        /// The post sign out automatic redirect delay.
        /// </value>
        public int PostSignOutAutoRedirectDelay { get; set; }

        /// <summary>
        /// Indicates if user must be authenticated to accept parameters to end session endpoint. Defaults to false.
        /// </summary>
        /// <value>
        /// <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool RequireAuthenticatedUserForSignOutMessage { get; set; }

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

        /// <summary>
        /// Gets or sets the limit after which old signin messages are purged.
        /// Defaults to the value defined in <see cref="Constants.SignInMessageThreshold"/> value.
        /// </summary>
        /// <value>
        /// The limit after which old signin messages are purged
        /// </value>
        public int SignInMessageThreshold { get; set; }

        /// <summary>
        /// Gets or sets the invalid sign in redirect URL. If the user arrives at the login page without
        /// a valid sign-in request, then they will be redirected to this URL. The URL must be absolute or
        /// can relative URLs (starting with "~/").
        /// </summary>
        /// <value>
        /// The invalid sign in redirect URL.
        /// </value>
        public string InvalidSignInRedirectUrl { get; set; }
    }
}