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

using IdentityServer3.Core.Models;

namespace IdentityServer3.Core.Events
{
    /// <summary>
    /// Event details for token revocation event
    /// </summary>
    public class TokenRevokedDetails
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token that was revoked.
        /// </value>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the token toke.
        /// </summary>
        /// <value>
        /// The type of token that was revoked. Access token or Refresh.
        /// </value>
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the subject Id
        /// </summary>
        /// <value></value>
        public string SubjectId { get; set; }
    }
}