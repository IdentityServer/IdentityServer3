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

using IdentityServer3.Core.Models;

namespace IdentityServer3.Core.Events
{
    /// <summary>
    /// Event details for login events
    /// </summary>
    public class LoginDetails : AuthenticationDetails
    {
        /// <summary>
        /// Gets or sets the sign in identifier.
        /// </summary>
        /// <value>
        /// The sign in identifier.
        /// </value>
        public string SignInId { get; set; }

        /// <summary>
        /// Gets or sets the sign in message.
        /// </summary>
        /// <value>
        /// The sign in message.
        /// </value>
        public SignInMessage SignInMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether login was a partial login.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is a partial login; otherwise, <c>false</c>.
        /// </value>
        public bool PartialLogin { get; set; }      
    }
}