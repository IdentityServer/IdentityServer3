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

namespace IdentityServer3.Core.Events
{
    /// <summary>
    /// Models base class for events raised from IdentityServer.
    /// </summary>
    public class Event<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event{T}" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException">category</exception>
        public Event(string category, string name, EventTypes type, int id, string message = null)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException("category");

            Category = category;
            Name = name;
            EventType = type;
            Id = id;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Event{T}" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="details">The details.</param>
        /// <param name="message">The message.</param>
        public Event(string category, string name, EventTypes type, int id, T details, string message = null)
            : this(category, name, type, id, message)
        {
            Details = details;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Event{T}" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="detailsFunc">The details function.</param>
        /// <param name="message">The message.</param>
        public Event(string category, string name, EventTypes type, int id, Func<T> detailsFunc, string message = null)
            : this(category, name, type, id, message)
        {
            DetailsFunc = detailsFunc;
        }

        /// <summary>
        /// Gets or sets the details function.
        /// </summary>
        /// <value>
        /// The details function.
        /// </value>
        [Newtonsoft.Json.JsonIgnore]
        public Func<T> DetailsFunc { get; set; }

        /// <summary>
        /// Allows event to defer data initialization until the event will be raised.
        /// </summary>
        internal void Prepare()
        {
            if (DetailsFunc != null)
            {
                Details = DetailsFunc();
            }
        }

        /// <summary>
        /// Gets or sets the event category. <see cref="EventConstants.Categories"/> for a list of the defined categories.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public EventTypes EventType { get; set; }

        /// <summary>
        /// Gets or sets the event identifier. <see cref="EventConstants.Ids"/> for the list of the defined identifiers.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

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
        public T Details { get; set; }

        /// <summary>
        /// Gets or sets the event context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public EventContext Context { get; set; }
    }
}