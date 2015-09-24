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

using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Validation;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Models the data to create a token from a validated request.
    /// </summary>
    public class TokenCreationRequest
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<Scope> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the validated request.
        /// </summary>
        /// <value>
        /// The validated request.
        /// </value>
        public ValidatedRequest ValidatedRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include all identity claims].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include all identity claims]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAllIdentityClaims { get; set; }

        /// <summary>
        /// Gets or sets the access token to hash.
        /// </summary>
        /// <value>
        /// The access token to hash.
        /// </value>
        public string AccessTokenToHash { get; set; }

        /// <summary>
        /// Gets or sets the authorization code to hash.
        /// </summary>
        /// <value>
        /// The authorization code to hash.
        /// </value>
        public string AuthorizationCodeToHash { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        /// <value>
        /// The nonce.
        /// </value>
        public string Nonce { get; set; }

        internal void Validate()
        {
            if (Client == null) LogAndStop("client");
            if (Scopes == null) LogAndStop("scopes");
            if (ValidatedRequest == null) LogAndStop("validatedRequest");
        }

        private void LogAndStop(string name)
        {
            Logger.ErrorFormat("{0} is null", name);
            throw new ArgumentNullException(name);
        }
    }
}