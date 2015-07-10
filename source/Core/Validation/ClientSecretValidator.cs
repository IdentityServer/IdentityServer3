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
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class ClientSecretValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IClientStore _clients;
        private readonly OwinEnvironmentService _environment;
        private readonly IEnumerable<ISecretParser> _parsers;
        private readonly IEnumerable<ISecretValidator> _validators;

        public ClientSecretValidator(IClientStore clients, IEnumerable<ISecretParser> parsers, IEnumerable<ISecretValidator> validators, OwinEnvironmentService environment)
        {
            _clients = clients;
            _parsers = parsers;
            _validators = validators;
            _environment = environment;
        }

        public async Task<ClientSecretValidationResult> ValidateAsync()
        {
            Logger.Debug("Start client validation");

            var fail = new ClientSecretValidationResult
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
                    Logger.DebugFormat("Parser found client secret: {0}", parser.GetType().Name);
                    Logger.InfoFormat("Client secret id found: {0}", parsedSecret.Id);

                    break;
                }
            }

            if (parsedSecret == null)
            {
                Logger.Info("No client secret found");
                return fail;
            }

            // load client
            var client = await _clients.FindClientByIdAsync(parsedSecret.Id);
            if (client == null)
            {
                Logger.Info("No client with that id found. aborting");
                return fail;
            }

            // see if a registered validator can validate the secret
            foreach (var validator in _validators)
            {
                var secretValidationResult = await validator.ValidateAsync(client.ClientSecrets, parsedSecret);

                if (secretValidationResult.Success)
                {
                    Logger.DebugFormat("Secret validator success: {0}", validator.GetType().Name);
                    Logger.Info("Client validation success");

                    var success = new ClientSecretValidationResult
                    {
                        IsError = false,
                        Client = client
                    };

                    return success;
                }
            }

            Logger.Info("Client validation failed.");
            return fail;
        }
    }
}