/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default client secret validator.
    /// </summary>
    public class DefaultClientSecretValidator : IClientSecretValidator
    {
        /// <summary>
        /// Validates the client secret
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="secret">The client secret.</param>
        /// <returns></returns>
        public Task<bool> ValidateClientSecretAsync(Client client, string secret)
        {
            if (client.ClientSecretProtection == ClientSecretProtection.Hashed)
            {
                using (var sha = SHA256.Create())
                {
                    var secretBytes = Encoding.UTF8.GetBytes(secret);
                    var hashed = sha.ComputeHash(secretBytes);

                    secret = Convert.ToBase64String(hashed);
                }
            }

            foreach (var clientSecret in client.ClientSecrets)
            {
                // check if client secret is still valid
                if (clientSecret.Expiration.HasValue)
                {
                    if (clientSecret.Expiration < DateTimeOffset.UtcNow)
                    {
                        // skip expired secrets
                        continue;
                    }
                }

                // use time constant string comparison
                var isValid = ObfuscatingComparer.IsEqual(clientSecret.Value, secret);

                if (isValid)
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}