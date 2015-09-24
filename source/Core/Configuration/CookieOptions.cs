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

using System;

namespace IdentityServer3.Core.Configuration
{
    /// <summary>
    /// Configured how cookies are managed by IdentityServer.
    /// </summary>
    public class CookieOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CookieOptions"/> class.
        /// </summary>
        public CookieOptions()
        {
            ExpireTimeSpan = Constants.DefaultCookieTimeSpan;
            SlidingExpiration = false;
            AllowRememberMe = true;
            RememberMeDuration = Constants.DefaultRememberMeDuration;
            SecureMode = CookieSecureMode.SameAsRequest;
        }

        /// <summary>
        /// Allows setting a prefix on cookies to avoid potential conflicts with other cookies with the same names.
        /// </summary>
        /// <value>
        /// The prefix.
        /// </value>
        public string Prefix { get; set; }
        
        /// <summary>
        /// The expiration duration of the authentication cookie. Defaults to 10 hours.
        /// </summary>
        /// <value>
        /// The expire time span.
        /// </value>
        public TimeSpan ExpireTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the authentication cookie is marked as persistent. Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if persistent; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersistent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the authentication cookie is sliding, which means it auto renews as the user is active. Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if sliding; otherwise, <c>false</c>.
        /// </value>
        public bool SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets the cookie path.
        /// </summary>
        /// <value>
        /// The cookie path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "remember me" option is presented to users on the login page. 
        /// If selected this option will issue a persistent authentication cookie. Defaults to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRememberMe { get; set; }

        /// <summary>
        /// Gets or sets the duration of the persistent cookie issued by the "remember me" option on the login page.
        /// Defaults to 30 days.
        /// </summary>
        /// <value>
        /// The duration of the "remember me" persistent cookie.
        /// </value>
        public TimeSpan RememberMeDuration { get; set; }

        /// <summary>
        /// Gets or sets the mode for issuing the secure flag on the cookies issued. Defaults to SameAsRequest.
        /// </summary>
        /// <value>
        /// The secure.
        /// </value>
        public CookieSecureMode SecureMode { get; set; }
    }
}
