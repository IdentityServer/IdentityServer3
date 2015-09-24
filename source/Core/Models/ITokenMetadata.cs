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
    /// Models the permissions contained within a token.
    /// </summary>
    public interface ITokenMetadata
    {
        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <value>
        /// The subject identifier.
        /// </value>
        string SubjectId { get; }
        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        string ClientId { get; }
        /// <summary>
        /// Gets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        IEnumerable<string> Scopes { get; }
    }
}