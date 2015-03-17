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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ClientValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        private readonly IClientStore _clients;
        private readonly IClientSecretValidator _secretValidator;
        private readonly OwinEnvironmentService _owin;

        private readonly ClientValidationLog _log;
        
        public ClientValidator(IClientStore clients, IClientSecretValidator secretValidator, OwinEnvironmentService owin)
        {
            _clients = clients;
            _secretValidator = secretValidator;
            _owin = owin;

            _log = new ClientValidationLog();
        }

        public async Task<Client> ValidateClientAsync(NameValueCollection parameters, AuthenticationHeaderValue header)
        {
            Logger.Info("Start client validation");

            // validate client credentials on the wire
            var credential = ValidateHttpRequest(header, parameters);

            if (credential.IsMalformed || !credential.IsPresent)
            {
                LogError("No or malformed client credential found.");
                return null;
            }

            _log.ClientId = credential.ClientId;
            _log.ClientCredentialType = credential.AuthenticationMethod.ToString();

            // validate client against configuration store
            var client = await ValidateClientCredentialsAsync(credential);
            if (client == null)
            {
                return null;
            }

            _log.ClientName = client.ClientName;

            LogSuccess();
            return client;
        }

        public ClientCredential ValidateHttpRequest(AuthenticationHeaderValue header, NameValueCollection body)
        {
            // shared secret via basic authentication
            var credentials = ParseBasicAuthenticationScheme(header);

            if (credentials.IsPresent && !credentials.IsMalformed)
            {
                return credentials;
            }

            if (credentials.IsMalformed)
            {
                return credentials;
            }

            // shared secret via post body
            credentials = ParsePostBody(body);

            if (credentials.IsPresent && !credentials.IsMalformed)
            {
                return credentials;
            }

            if (credentials.IsMalformed)
            {
                return credentials;
            }

            return ParseX509ClientCertificate(body);
        }

        public async Task<Client> ValidateClientCredentialsAsync(ClientCredential credential)
        {
            if (credential == null || 
                !credential.IsPresent)
            {
                throw new InvalidOperationException("credential is not present");
            }

            var client = await _clients.FindClientByIdAsync(credential.ClientId);
            if (client == null || client.Enabled == false)
            {
                LogError("Client not found in registry or not enabled");
                return null;
            }

            var secretValid = await _secretValidator.ValidateClientSecretAsync(client, credential);
            if (secretValid == false)
            {
                LogError("Invalid client secret");
                return null;
            }

            return client;
        }

        private ClientCredential ParseBasicAuthenticationScheme(AuthenticationHeaderValue header)
        {
            if (header == null ||
                !header.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                return new ClientCredential
                {
                    IsPresent = false,
                    IsMalformed = false
                };
            }

            string pair;
            try
            {
                pair = Encoding.UTF8.GetString(
                    Convert.FromBase64String(header.Parameter));
            }
            catch (FormatException)
            {
                return new ClientCredential
                {
                    IsPresent = false,
                    IsMalformed = true
                };
            }
            catch (ArgumentException)
            {
                return new ClientCredential
                {
                    IsPresent = false,
                    IsMalformed = true
                };
            }

            var ix = pair.IndexOf(':');
            if (ix == -1)
            {
                return new ClientCredential
                {
                    IsPresent = false,
                    IsMalformed = true
                };
            }

            var clientId = pair.Substring(0, ix);
            var secret = pair.Substring(ix + 1);

            if (clientId.IsPresent() && secret.IsPresent())
            {
                return new ClientCredential
                {
                    ClientId = clientId,
                    SharedSecret = secret,

                    IsPresent = true,
                    IsMalformed = false,
                    AuthenticationMethod = ClientAuthenticationMethods.Basic
                };
            }

            return new ClientCredential
            {
                IsMalformed = true,
                IsPresent = false
            };
        }

        private ClientCredential ParsePostBody(NameValueCollection body)
        {
            if (body == null)
            {
                return new ClientCredential
                {
                    IsPresent = false
                };
            }

            var id = body.Get("client_id");
            var secret = body.Get("client_secret");

            if (id.IsPresent() && secret.IsPresent())
            {
                return new ClientCredential
                {
                    ClientId = id,
                    SharedSecret = secret,

                    IsMalformed = false,
                    IsPresent = true,
                    AuthenticationMethod = ClientAuthenticationMethods.FormPost
                };
            }

            return new ClientCredential
            {
                IsPresent = false
            };
        }

        private ClientCredential ParseX509ClientCertificate(NameValueCollection body)
        {
            var credential = new ClientCredential
            {
                IsPresent = false
            };

            if (body == null)
            {
                return credential;
            }

            var id = body.Get("client_id");
            if (id.IsMissing())
            {
                return credential;
            }

            var context = new OwinContext(_owin.Environment);
            var cert = context.Get<X509Certificate2>("ssl.ClientCertificate");

            if (cert != null)
            {
                return new ClientCredential
                {
                    IsPresent = true,
                    IsMalformed = false,

                    ClientId = id,
                    ClientCertificate = cert,
                    AuthenticationMethod = ClientAuthenticationMethods.X509Certificate
                };
            }

            return credential;
        }

        private void LogError(string message)
        {
            var json = LogSerializer.Serialize(_log);
            Logger.ErrorFormat("{0}\n {1}", message, json);
        }

        private void LogSuccess()
        {
            var json = LogSerializer.Serialize(_log);
            Logger.InfoFormat("{0}\n {1}", "Client validation success", json);
        }
    }
}