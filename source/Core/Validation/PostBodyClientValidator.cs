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
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Client validator for client secrets posted in the body
    /// </summary>
    public class PostBodyClientValidator : ClientValidatorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostBodyClientValidator"/> class.
        /// </summary>
        /// <param name="secretValidator">The secret validator.</param>
        /// <param name="clients">The client store.</param>
        public PostBodyClientValidator(IClientSecretValidator secretValidator, IClientStore clients)
            : base(secretValidator, clients)
        { }

        /// <summary>
        /// Extracts the credential from the HTTP request.
        /// </summary>
        /// <param name="environment">The OWIN environment.</param>
        /// <returns></returns>
        public override async Task<ClientCredential> ExtractCredentialAsync(IDictionary<string, object> environment)
        {
            var context = new OwinContext(environment);

            var credential = new ClientCredential
            {
                CredentialType = Constants.ClientCredentialTypes.SharedSecret,
                IsPresent = false
            };

            var body = await context.ReadRequestFormAsync();

            if (body != null)
            {
                var id = body.Get("client_id");
                var secret = body.Get("client_secret");

                if (id.IsPresent() && secret.IsPresent())
                {
                    credential.IsPresent = true;
                    credential.ClientId = id;
                    credential.Credential = secret;

                    return credential;
                }
            }

            return credential;
        }
    }
}