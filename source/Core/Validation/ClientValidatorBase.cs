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

using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public abstract class ClientValidatorBase : IClientValidator
    {
        protected readonly IClientSecretValidator _secretValidator;
        protected readonly IClientStore _clients;

        protected abstract Task<ClientCredential> ExtractCredentialAsync(IDictionary<string, object> environment);

        public ClientValidatorBase(IClientSecretValidator secretValidator, IClientStore clients)
        {
            _secretValidator = secretValidator;
            _clients = clients;
        }

        public async Task<ClientValidationResult> ValidateAsync(IDictionary<string, object> environment)
        {
            var credential = await ExtractCredentialAsync(environment);

            if (credential.IsPresent)
            {
                var client = await _clients.FindClientByIdAsync(credential.ClientId);
                if (client == null)
                {
                    return new ClientValidationResult
                    {
                        IsError = true,
                        Error = "Unknown client."
                    };
                }

                if (await _secretValidator.ValidateClientSecretAsync(client, credential))
                {
                    return new ClientValidationResult
                    {
                        IsError = false,
                        Client = client
                    };
                }
            }

            return new ClientValidationResult
            {
                IsError = true,
                Error = "Invalid or malformed client credentials."
            };
        }
    }
}