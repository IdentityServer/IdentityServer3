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

using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class BasicAuthenticationClientValidator : ClientValidatorBase
    {
        public BasicAuthenticationClientValidator(IClientSecretValidator secretValidator, IClientStore clients)
            : base(secretValidator, clients)
        { }

        public override Task<ClientCredential> ExtractCredentialAsync(IDictionary<string, object> environment)
        {
            var credential = new ClientCredential
            {
                CredentialType = Constants.ClientCredentialTypes.SharedSecret,
                IsPresent = false
            };

            var context = new OwinContext(environment);
            var authorizationHeader = context.Request.Headers.Get("Authorization");

            if (authorizationHeader == null)
            {
                return Task.FromResult(credential);
            }

            if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(credential);
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
                return Task.FromResult(credential);
            }
            catch (ArgumentException)
            {
                return Task.FromResult(credential);
            }

            var ix = pair.IndexOf(':');
            if (ix == -1)
            {
                return Task.FromResult(credential);
            }

            var clientId = pair.Substring(0, ix);
            var secret = pair.Substring(ix + 1);

            if (clientId.IsPresent() && secret.IsPresent())
            {
                credential.IsPresent = true;
                credential.ClientId = clientId;
                credential.Credential = secret;

                return Task.FromResult(credential);
            }

            return Task.FromResult(credential);
        }
    }
}