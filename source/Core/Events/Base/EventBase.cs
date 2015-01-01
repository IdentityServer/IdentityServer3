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
    /// Models base class for events raised from IdentityServer.
    /// </summary>
    public class EventBase
    {
        /// <summary>
        /// Gets or sets the event context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public EventContext Context { get; set; }

        /// <summary>
        /// Gets or sets the event identifier. <see cref="EventConstants.Ids"/> for the list of the defined identifiers.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public EventType EventType { get; set; }
        
        /// <summary>
        /// Gets or sets the event category. <see cref="EventConstants.Categories"/> for a list of the defined categories.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }
        
        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the event details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public string Details { get; set; }
    }
}