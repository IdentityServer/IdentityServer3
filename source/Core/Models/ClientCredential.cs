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

using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.IdentityServer.Core.Models
{
    /// <summary>
    /// Models a client credential
    /// </summary>
    public class ClientCredential
    {
        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        /// <value>
        /// The client id.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the shared secret (if shared secrets are used).
        /// </summary>
        /// <value>
        /// The shared secret.
        /// </value>
        public object Secret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a secret is present.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is present; otherwise, <c>false</c>.
        /// </value>
        public bool IsPresent { get; set; }

        /// <summary>
        /// Gets or sets the authentication method.
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        public string CredentialType { get; set; }
    }
}