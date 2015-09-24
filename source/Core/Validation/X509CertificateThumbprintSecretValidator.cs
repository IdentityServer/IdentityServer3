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
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Validates a secret based on the thumbprint of an X509 Certificate
    /// </summary>
    public class X509CertificateThumbprintSecretValidator : ISecretValidator
    {
        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>
        /// A validation result
        /// </returns>
        /// <exception cref="System.ArgumentException">ParsedSecret.Credential is not an X509 Certificate</exception>
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });
            var success = Task.FromResult(new SecretValidationResult { Success = true });

            if (parsedSecret.Type != Constants.ParsedSecretTypes.X509Certificate)
            {
                return fail;
            }

            var cert = parsedSecret.Credential as X509Certificate2;

            if (cert == null)
            {
                throw new ArgumentException("ParsedSecret.Credential is not an X509 Certificate");
            }

            var thumbprint = cert.Thumbprint;

            foreach (var secret in secrets)
            {
                // check if client secret is still valid
                if (secret.Expiration.HasExpired()) continue;

                if (secret.Type == Constants.SecretTypes.X509CertificateThumbprint)
                {
                    if (TimeConstantComparer.IsEqual(thumbprint.ToLowerInvariant(), secret.Value.ToLowerInvariant()))
                    {
                        return success;
                    }
                }
            }

            return fail;
        }
    }
}