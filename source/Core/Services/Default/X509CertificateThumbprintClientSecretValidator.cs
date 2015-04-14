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

using IdentityModel;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Client secret validator based on X.509 certificate thumbprints
    /// </summary>
    public class X509CertificateThumbprintClientSecretValidator : IClientSecretValidator
    {
        /// <summary>
        /// Validates the client secret.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="credential">The client credential.</param>
        /// <returns>
        ///   <c>true</c> if the secret is valid; <c>false</c> otherwise.
        /// </returns>
        public Task<bool> ValidateClientSecretAsync(Client client, ClientCredential credential)
        {
            if (credential.CredentialType != Constants.ClientCredentialTypes.X509Certificate)
            {
                return Task.FromResult(false);
            }

            var cert = credential.Credential as X509Certificate2;

            if (cert == null)
            {
                throw new ArgumentNullException("ClientCredential.Credential");
            }

            var thumbprint = cert.Thumbprint;

            foreach (var secret in client.ClientSecrets)
            {
                // check if client secret is still valid
                if (secret.Expiration.HasExpired()) continue;

                if (secret.Type == Constants.SecretTypes.X509CertificateThumbprint)
                {
                    if (TimeConstantComparer.IsEqual(thumbprint.ToLowerInvariant(), secret.Value.ToLowerInvariant()))
                    {
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }
    }
}