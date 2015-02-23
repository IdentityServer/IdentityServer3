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
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Validation
{
    /// <summary>
    /// Modles the validation result of access tokens and id tokens.
    /// </summary>
    public class TokenValidationResult
    {
        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<Claim> Claims { get; set; }
        /// <summary>
        /// Gets or sets the JWT.
        /// </summary>
        /// <value>
        /// The JWT.
        /// </value>
        public string Jwt { get; set; }
        /// <summary>
        /// Gets or sets the reference token.
        /// </summary>
        /// <value>
        /// The reference token.
        /// </value>
        public Token ReferenceToken { get; set; }
        /// <summary>
        /// Gets or sets the reference token identifier.
        /// </summary>
        /// <value>
        /// The reference token identifier.
        /// </value>
        public string ReferenceTokenId { get; set; }
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </value>
        public bool IsError { get; set; }
    }
}