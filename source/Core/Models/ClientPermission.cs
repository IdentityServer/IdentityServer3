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

using System.Collections.Generic;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Models permissions granted to a client.
    /// </summary>
    public class ClientPermission
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }
        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }
        /// <summary>
        /// Gets or sets the client URL.
        /// </summary>
        /// <value>
        /// The client URL.
        /// </value>
        public string ClientUrl { get; set; }
        /// <summary>
        /// Gets or sets the client logo URL.
        /// </summary>
        /// <value>
        /// The client logo URL.
        /// </value>
        public string ClientLogoUrl { get; set; }
        /// <summary>
        /// Gets or sets the identity permissions.
        /// </summary>
        /// <value>
        /// The identity permissions.
        /// </value>
        public IEnumerable<ClientPermissionDescription> IdentityPermissions { get; set; }
        /// <summary>
        /// Gets or sets the resource permissions.
        /// </summary>
        /// <value>
        /// The resource permissions.
        /// </value>
        public IEnumerable<ClientPermissionDescription> ResourcePermissions { get; set; }
    }
}
