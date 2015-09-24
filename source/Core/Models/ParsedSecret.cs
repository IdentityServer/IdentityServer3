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

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Represents a secret extracted from the OWIN environment
    /// </summary>
    public class ParsedSecret
    {
        /// <summary>
        /// Gets or sets the identifier associated with this secret
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the credential to verify the secret
        /// </summary>
        /// <value>
        /// The credential.
        /// </value>
        public object Credential { get; set; }

        /// <summary>
        /// Gets or sets the type of the secret
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
    }
}