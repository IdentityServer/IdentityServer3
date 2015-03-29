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
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Represents the information needed to issue a login cookie.
    /// </summary>
    public class AuthenticatedLogin
    {
        /// <summary>
        /// The subject claim used to uniquely identifier the user.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// The name claim used as the display name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Claims that will be maintained in the login.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<Claim> Claims { get; set; }

        /// <summary>
        /// The authentication method. This should be used when 
        /// local authentication is performed as some other means other than password has been 
        /// used to authenticate the user (e.g. '2fa' for two-factor, or 'certificate' for client 
        /// certificates).
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// The identity provider. This should used when an external
        /// identity provider is used and will typically match the <c>AuthenticationType</c> as configured
        /// on the Katana authentication middleware.
        /// </summary>
        /// <value>
        /// The identity provider.
        /// </value>
        public string IdentityProvider { get; set; }

        /// <summary>
        /// Gets or sets if the cookie should be persistent.
        /// </summary>
        /// <value>
        /// The persistent login.
        /// </value>
        public bool? PersistentLogin { get; set; }

        /// <summary>
        /// Gets or sets the expiration for the persistent cookie.
        /// </summary>
        /// <value>
        /// The persistent login expiration.
        /// </value>
        public DateTimeOffset? PersistentLoginExpiration { get; set; }
    }
}
