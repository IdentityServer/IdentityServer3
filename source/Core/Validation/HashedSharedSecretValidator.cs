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
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Validates a shared secret stored in SHA256 or SHA512
    /// </summary>
    public class HashedSharedSecretValidator : ISecretValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>
        /// A validation result
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Id or cedential</exception>
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });
            var success = Task.FromResult(new SecretValidationResult { Success = true });

            if (parsedSecret.Type == Constants.ParsedSecretTypes.SharedSecret)
            {
                var sharedSecret = parsedSecret.Credential as string;

                if (parsedSecret.Id.IsMissing() || sharedSecret.IsMissing())
                {
                    throw new ArgumentNullException("Id or cedential");
                }

                var secretSha256 = sharedSecret.Sha256();
                var secretSha512 = sharedSecret.Sha512();

                foreach (var secret in secrets)
                {
                    // this validator is only applicable to shared secrets
                    if (secret.Type != Constants.SecretTypes.SharedSecret)
                    {
                        continue;
                    }

                    bool isValid = false;
                    byte[] secretBytes;

                    // check if client secret is still valid
                    if (secret.Expiration.HasExpired()) continue;

                    try
                    {
                        secretBytes = Convert.FromBase64String(secret.Value);
                    }
                    catch (FormatException)
                    {
                        Logger.Error("Secret uses invalid hashing algorithm");
                        return fail;
                    }

                    if (secretBytes.Length == 32)
                    {
                        isValid = TimeConstantComparer.IsEqual(secret.Value, secretSha256);
                    }
                    else if (secretBytes.Length == 64)
                    {
                        isValid = TimeConstantComparer.IsEqual(secret.Value, secretSha512);
                    }
                    else
                    {
                        Logger.Error("Secret uses invalid hashing algorithm");
                        return fail;
                    }

                    if (isValid)
                    {
                        return success;
                    }
                }
            }

            return fail;
        }
    }
}