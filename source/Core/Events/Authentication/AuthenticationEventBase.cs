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

using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Events
{
    /// <summary>
    /// Base class for authentication events
    /// </summary>
    public abstract class AuthenticationEventBase : EventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationEventBase"/> class.
        /// </summary>
        /// <param name="id">Event id.</param>
        /// <param name="type">Event type.</param>
        protected AuthenticationEventBase(int id, EventTypes type) : base(EventConstants.Categories.Authentication)
        {
            Id = id;
            EventType = type;
        }

        /// <summary>
        /// Gets or sets the subject identifier.
        /// </summary>
        /// <value>
        /// The subject identifier.
        /// </value>
        public string SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

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