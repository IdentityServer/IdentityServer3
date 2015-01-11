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

using System;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default client secret validator.
    /// </summary>
    public class HashedClientSecretValidator : IClientSecretValidator
    {
        /// <summary>
        /// Validates the client secret
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="secret">The client secret.</param>
        /// <returns></returns>
        public virtual Task<bool> ValidateClientSecretAsync(Client client, string secret)
        {
            var secretSha256 = secret.Sha256();
            var secretSha512 = secret.Sha512();

            foreach (var clientSecret in client.ClientSecrets)
            {
                bool isValid = false;
                byte[] clientSecretBytes;

                // check if client secret is still valid
                if (clientSecret.Expiration.HasExpired()) continue;

                try
                {
                    clientSecretBytes = Convert.FromBase64String(clientSecret.Value);
                }
                catch (FormatException)
                {
                    // todo: logging
                    throw new InvalidOperationException("Invalid hashing algorithm for client secret.");
                }
                
                if (clientSecretBytes.Length == 32)
                {
                    isValid = ObfuscatingComparer.IsEqual(clientSecret.Value, secretSha256);
                }
                else if (clientSecretBytes.Length == 64)
                {
                    isValid = ObfuscatingComparer.IsEqual(clientSecret.Value, secretSha512);
                }
                else
                {
                    // todo: logging
                    throw new InvalidOperationException("Invalid hashing algorithm for client secret.");
                }

                if (isValid)
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}