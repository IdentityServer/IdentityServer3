/*
 * Copyright 2014 Dominick Baier, Brock Allen
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


namespace Thinktecture.IdentityServer.Core.Events
{
    /// <summary>
    /// Event class for local login events
    /// </summary>
    public class LocalLoginEvent : AuthenticationEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalLoginEvent"/> class.
        /// </summary>
        /// <param name="type">The event type.</param>
        public LocalLoginEvent(EventType type)
            : base(EventConstants.Ids.LocalLogin, type)
        {
            if (type == EventType.Success)
            {
                Message = Resources.Events.LocalLoginSuccess;
            }
            else if (type == EventType.Failure)
            {
                Message = Resources.Events.LocalLoginFailure;
            }
            else
            {
                Message = "Local login event";
            }
        }

        /// <summary>
        /// Gets or sets the name of the login user.
        /// </summary>
        /// <value>
        /// The name of the login user.
        /// </value>
        public string LoginUserName { get; set; }
    }
}