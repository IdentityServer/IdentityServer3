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
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class ScopeSecretValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IScopeStore _scopes;
        private readonly OwinEnvironmentService _environment;
        private readonly IEventService _events;
        private readonly SecretParser _parser;
        private readonly SecretValidator _validator;

        public ScopeSecretValidator(IScopeStore scopes, SecretParser parsers, SecretValidator validator, OwinEnvironmentService environment, IEventService events)
        {
            _scopes = scopes;
            _environment = environment;
            _parser = parsers;
            _validator = validator;
            _events = events;
        }

        public async Task<ScopeSecretValidationResult> ValidateAsync()
        {
            Logger.Debug("Start scope validation");

            var fail = new ScopeSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(_environment.Environment);
            if (parsedSecret == null)
            {
                await RaiseFailureEvent("unknown", "No scope id or secret found");

                Logger.Info("No scope secret found");
                return fail;
            }

            // load scope
            var scope = (await _scopes.FindScopesAsync(new[] { parsedSecret.Id })).FirstOrDefault();
            if (scope == null)
            {
                await RaiseFailureEvent(parsedSecret.Id, "Unknown scope");

                Logger.Info("No scope with that name found. aborting");
                return fail;
            }

            var result = await _validator.ValidateAsync(parsedSecret, scope.ScopeSecrets);
            if (result.Success)
            {
                Logger.Info("Scope validation success");

                var success = new ScopeSecretValidationResult
                {
                    IsError = false,
                    Scope = scope
                };

                await RaiseSuccessEvent(scope.Name);
                return success;
            }

            await RaiseFailureEvent(scope.Name, "Invalid client secret");
            Logger.Info("Scope validation failed.");

            return fail;
        }

        private async Task RaiseSuccessEvent(string clientId)
        {
            await _events.RaiseSuccessfulClientAuthenticationEventAsync(clientId, EventConstants.ClientTypes.Scope);
        }

        private async Task RaiseFailureEvent(string clientId, string message)
        {
            await _events.RaiseFailureClientAuthenticationEventAsync(message, clientId, EventConstants.ClientTypes.Scope);
        }
    }
}