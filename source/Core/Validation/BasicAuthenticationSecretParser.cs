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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Parses a Basic Authentication header
    /// </summary>
    public class BasicAuthenticationSecretParser : ISecretParser
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Tries to find a secret on the environment that can be used for authentication
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            Logger.Debug("Start parsing Basic Authentication secret");

            var notfound = Task.FromResult<ParsedSecret>(null);
            var context = new OwinContext(environment);
            var authorizationHeader = context.Request.Headers.Get("Authorization");

            if (authorizationHeader == null)
            {
                return notfound;
            }

            if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return notfound;
            }

            var parameter = authorizationHeader.Substring("Basic ".Length);

            string pair;
            try
            {
                pair = Encoding.UTF8.GetString(
                    Convert.FromBase64String(parameter));
            }
            catch (FormatException)
            {
                Logger.Debug("Malformed Basic Authentication credential.");
                return notfound;
            }
            catch (ArgumentException)
            {
                Logger.Debug("Malformed Basic Authentication credential.");
                return notfound;
            }

            var ix = pair.IndexOf(':');
            if (ix == -1)
            {
                Logger.Debug("Malformed Basic Authentication credential.");
                return notfound;
            }

            var clientId = pair.Substring(0, ix);
            var secret = pair.Substring(ix + 1);

            if (clientId.IsPresent() && secret.IsPresent())
            {
                var parsedSecret = new ParsedSecret
                {
                    Id = clientId,
                    Credential = secret,
                    Type = Constants.ParsedSecretTypes.SharedSecret
                };

                return Task.FromResult(parsedSecret);
            }

            Logger.Debug("No Basic Authentication secret found");
            return notfound;
        }
    }
}