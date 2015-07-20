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
using System.Linq;

namespace IdentityServer3.Core.Validation
{
    internal class ScopeSecretValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IScopeStore _scopes;
        private readonly OwinEnvironmentService _environment;
        private readonly IEnumerable<ISecretParser> _parsers;
        private readonly IEnumerable<ISecretValidator> _validators;

        public ScopeSecretValidator(IScopeStore scopes, IEnumerable<ISecretParser> parsers, IEnumerable<ISecretValidator> validators, OwinEnvironmentService environment)
        {
            _scopes = scopes;
            _parsers = parsers;
            _validators = validators;
            _environment = environment;
        }

        public async Task<ScopeSecretValidationResult> ValidateAsync()
        {
            Logger.Debug("Start scope validation");

            var fail = new ScopeSecretValidationResult
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
                    Logger.DebugFormat("Parser found scope secret: {0}", parser.GetType().Name);
                    Logger.InfoFormat("Scope name found: {0}", parsedSecret.Id);

                    break;
                }
            }

            if (parsedSecret == null)
            {
                Logger.Info("No scope secret found");
                return fail;
            }

            // load client
            var scope = (await _scopes.FindScopesAsync(new[] { parsedSecret.Id })).FirstOrDefault();
            if (scope == null)
            {
                Logger.Info("No scope with that name found. aborting");
                return fail;
            }

            // see if a registered validator can validate the secret
            foreach (var validator in _validators)
            {
                var secretValidationResult = await validator.ValidateAsync(scope.ScopeSecrets, parsedSecret);

                if (secretValidationResult.Success)
                {
                    Logger.DebugFormat("Secret validator success: {0}", validator.GetType().Name);
                    Logger.Info("Scope validation success");

                    var success = new ScopeSecretValidationResult
                    {
                        IsError = false,
                        Scope = scope
                    };

                    return success;
                }
            }

            Logger.Info("Scope validation failed.");
            return fail;
        }
    }
}