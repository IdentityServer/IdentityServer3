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
using IdentityServer3.Core.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    internal class ClientValidator
    {
        private readonly IClientStore _clients;
        private readonly OwinEnvironmentService _environment;
        private readonly IEnumerable<ISecretParser> _parsers;
        private readonly IEnumerable<ISecretValidator> _validators;

        public ClientValidator(IClientStore clients, IEnumerable<ISecretParser> parsers, IEnumerable<ISecretValidator> validators, OwinEnvironmentService environment)
        {
            _clients = clients;
            _parsers = parsers;
            _validators = validators;
            _environment = environment;
        }

        public async Task<ClientSecretValidationResult> ValidateAsync()
        {
            var result = new ClientSecretValidationResult
            {
                IsError = true
            };

            // see if a registered parser finds a secret on the request
            ParsedSecret parsedSecret = null;
            foreach (var parser in _parsers)
            {
                parsedSecret = await parser.ParseAsync(_environment.Environment);
                if (parsedSecret != null)
                {
                    break;
                }
            }

            if (parsedSecret == null)
            {
                return result;
            }

            // load client
            var client = await _clients.FindClientByIdAsync(parsedSecret.Id);
            if (client == null)
            {
                return result;
            }

            // see if a registered validator can validate the secret
            SecretValidationResult secretValidationResult = null;
            foreach (var validator in _validators)
            {
                secretValidationResult = await validator.ValidateAsync(client.ClientSecrets, parsedSecret);
                if (secretValidationResult.Success)
                {
                    result.Client = client;
                    result.IsError = false;

                    return result;
                }
            }

            return result;
        }
    }
}