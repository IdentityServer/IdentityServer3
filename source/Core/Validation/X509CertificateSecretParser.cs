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
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Parses the environment for an X509 client certificate
    /// </summary>
    public class X509CertificateSecretParser : ISecretParser
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger(); 

        /// <summary>
        /// Tries to find a secret on the environment that can be used for authentication
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            Logger.Debug("Start parsing for X.509 certificate");

            var context = new OwinContext(environment);
            var body = await context.ReadRequestFormAsync();

            if (body == null)
            {
                return null;
            }

            var id = body.Get("client_id");
            if (id.IsMissing())
            {
                Logger.Debug("client_id is not found in post body");
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

            Logger.Debug("X.509 certificate not found.");
            return null;
        }
    }
}