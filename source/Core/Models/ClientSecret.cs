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

namespace Thinktecture.IdentityServer.Core.Models
{
    /// <summary>
    /// Models a client secret with identifier and expiration
    /// </summary>
    public class ClientSecret
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>
        /// The expiration.
        /// </value>
        public DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// Gets or sets the type of the client secret.
        /// </summary>
        /// <value>
        /// The type of the client secret.
        /// </value>
        public string ClientSecretType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSecret"/> class.
        /// </summary>
        public ClientSecret()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSecret"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expiration">The expiration.</param>
        public ClientSecret(string value, DateTimeOffset? expiration = null)
        {
            Value = value;
            Expiration = expiration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSecret" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="expiration">The expiration.</param>
        public ClientSecret(string value, string description, DateTimeOffset? expiration = null)
        {
            Description = description;
            Value = value;
            Expiration = expiration;
        }
    }
}