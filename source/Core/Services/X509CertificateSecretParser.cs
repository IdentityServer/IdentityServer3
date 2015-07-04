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
using Microsoft.Owin;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services
{
    class X509CertificateSecretParser : ISecretParser
    {
        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            var context = new OwinContext(environment);
            var body = await context.ReadRequestFormAsync();

            if (body == null)
            {
                return null;
            }

            var id = body.Get("client_id");
            if (id.IsMissing())
            {
                return null;
            }

            var cert = context.Get<X509Certificate2>("ssl.ClientCertificate");

            if (cert != null)
            {
                return new ParsedSecret
                {
                    Id = id,
                    Credential = cert,
                    Type = Constants.ParsedSecretTypes.X509Certificate
                };
            }

            return null;
        }
    }
}