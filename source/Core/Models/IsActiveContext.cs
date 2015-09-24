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
using System.Security.Claims;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Context describing the is-active check
    /// </summary>
    public class IsActiveContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsActiveContext"/> class.
        /// </summary>
        public IsActiveContext(ClaimsPrincipal subject, Client client)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (client == null) throw new ArgumentNullException("client");

            Subject = subject;
            Client = client;
            
            IsActive = true;
        }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the subject is active and can recieve tokens.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the subject is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }
    }
}