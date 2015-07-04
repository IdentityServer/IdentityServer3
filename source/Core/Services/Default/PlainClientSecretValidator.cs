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
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Client secret validator for plain text secrets
    /// </summary>
    public class PlainTextClientSecretValidator : IClientSecretValidator
    {
        /// <summary>
        /// Validates the client secret
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="credential">The client credential.</param>
        /// <returns></returns>
        public virtual Task<bool> ValidateClientSecretAsync(Client client, ClientCredential credential)
        {
            if (credential.CredentialType == Constants.ParsedSecretTypes.SharedSecret)
            {
                if (credential.ClientId.IsMissing() || credential.Credential == null || credential.Credential.ToString().IsMissing())
                {
                    throw new ArgumentNullException("Credential.ClientId or Credential.Credential");
                }

                foreach (var clientSecret in client.ClientSecrets)
                {
                    // this validator is only applicable to shared secrets
                    if (clientSecret.Type != Constants.SecretTypes.SharedSecret)
                    {
                        continue;
                    }

                    // check if client secret is still valid
                    if (clientSecret.Expiration.HasExpired()) continue;

                    // use time constant string comparison
                    var isValid = TimeConstantComparer.IsEqual(clientSecret.Value, credential.Credential.ToString());

                    if (isValid)
                    {
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }
    }
}