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

using IdentityServer3.Core.Events;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class ClientSecretValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IClientStore _clients;
        private readonly OwinEnvironmentService _environment;
        private readonly IEventService _events;
        private readonly SecretParser _parser;
        private readonly SecretValidator _validator;

        public ClientSecretValidator(IClientStore clients, SecretParser parser, SecretValidator validator, OwinEnvironmentService environment, IEventService events)
        {
            _clients = clients;
            _parser = parser;
            _validator = validator;
            _environment = environment;
            _events = events;
        }

        public async Task<ClientSecretValidationResult> ValidateAsync()
        {
            Logger.Debug("Start client validation");

            var fail = new ClientSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(_environment.Environment);
            if (parsedSecret == null)
            {
                await RaiseFailureEvent("unknown", "No client id or secret found");

                Logger.Info("No client secret found");
                return fail;
            }

            // load client
            var client = await _clients.FindClientByIdAsync(parsedSecret.Id);
            if (client == null)
            {
                await RaiseFailureEvent(parsedSecret.Id, "Unknown client");

                Logger.Info("No client with that id found. aborting");
                return fail;
            }

            var result = await _validator.ValidateAsync(parsedSecret, client.ClientSecrets);

            if (result.Success)
            {
                Logger.Info("Client validation success");

                var success = new ClientSecretValidationResult
                {
                    IsError = false,
                    Client = client
                };

                await RaiseSuccessEvent(client.ClientId);
                return success;
            }

            await RaiseFailureEvent(client.ClientId, "Invalid client secret");
            Logger.Info("Client validation failed.");
            
            return fail;
        }

        private async Task RaiseSuccessEvent(string clientId)
        {
            await _events.RaiseSuccessfulClientAuthenticationEventAsync(clientId, EventConstants.ClientTypes.Client);
        }

        private async Task RaiseFailureEvent(string clientId, string message)
        {
            await _events.RaiseFailureClientAuthenticationEventAsync(message, clientId, EventConstants.ClientTypes.Client);
        }
    }
}