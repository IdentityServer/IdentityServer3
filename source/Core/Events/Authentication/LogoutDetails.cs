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
    /// Event details for logout events
    /// </summary>
    public class LogoutDetails : AuthenticationDetails
    {
        /// <summary>
        /// Gets or sets the sign out identifier.
        /// </summary>
        /// <value>
        /// The sign out identifier.
        /// </value>
        public string SignOutId { get; set; }

        /// <summary>
        /// Gets or sets the sign out message.
        /// </summary>
        /// <value>
        /// The sign out message.
        /// </value>
        public SignOutMessage SignOutMessage { get; set; }
    }
}