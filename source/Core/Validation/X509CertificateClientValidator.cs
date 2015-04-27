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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Client secret validator for X.509 client certificates
    /// </summary>
    public class X509CertificateClientValidator : ClientValidatorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X509CertificateClientValidator"/> class.
        /// </summary>
        /// <param name="secretValidator">The secret validator.</param>
        /// <param name="clients">The client store.</param>
        public X509CertificateClientValidator(IClientSecretValidator secretValidator, IClientStore clients)
            : base(secretValidator, clients)
        { }

        /// <summary>
        /// Extracts the credential from the HTTP request.
        /// </summary>
        /// <param name="environment">The OWIN environment.</param>
        /// <returns></returns>
        public override async Task<ClientCredential> ExtractCredentialAsync(IDictionary<string, object> environment)
        {
            var credential = new ClientCredential
            {
                IsPresent = false,
                CredentialType = Constants.ClientCredentialTypes.X509Certificate
            };

            var context = new OwinContext(environment);
            var body = await context.ReadRequestFormAsync();

            if (body == null)
            {
                return credential;
            }

            var id = body.Get("client_id");
            if (id.IsMissing())
            {
                return credential;
            }

            var cert = context.Get<X509Certificate2>("ssl.ClientCertificate");

            if (cert != null)
            {
                credential.IsPresent = true;
                credential.Credential = cert;
                credential.ClientId = id;

                return credential;
            }

            return credential;
        }
    }
}