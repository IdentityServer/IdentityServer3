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
using System.Collections.Generic;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Models a validated request to the token endpoint.
    /// </summary>
    public class ValidatedTokenRequest : ValidatedRequest
    {
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the grant.
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        public string GrantType { get; set; }
        
        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the username used in the request.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the sign in message.
        /// </summary>
        /// <value>
        /// The sign in message.
        /// </value>
        public SignInMessage SignInMessage { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public RefreshToken RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token handle.
        /// </summary>
        /// <value>
        /// The refresh token handle.
        /// </value>
        public string RefreshTokenHandle { get; set; }

        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        /// <value>
        /// The authorization code.
        /// </value>
        public AuthorizationCode AuthorizationCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization code handle.
        /// </summary>
        /// <value>
        /// The authorization code handle.
        /// </value>
        public string AuthorizationCodeHandle { get; set; }
    }
}