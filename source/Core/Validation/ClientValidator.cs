/*
 * Copyright 2014 Dominick Baier, Brock Allen
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

using System;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class ClientValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IClientStore _clients;

        public ClientValidator(IClientStore clients)
        {
            _clients = clients;
        }

        public async Task<Client> ValidateClientAsync(NameValueCollection parameters, AuthenticationHeaderValue header)
        {
            Logger.Info("Start client validation");

            // validate client credentials on the wire
            var credential = ValidateHttpRequest(header, parameters);

            if (credential.IsMalformed || !credential.IsPresent)
            {
                Logger.Error("No or malformed client credential found.");
                return null;
            }

            // validate client against configuration store
            var client = await ValidateClientCredentialsAsync(credential);
            if (client == null)
            {
                Logger.Error("Invalid client credentials. Aborting.");
                return null;
            }

            return client;
        }

        public ClientCredential ValidateHttpRequest(AuthenticationHeaderValue header, NameValueCollection body)
        {
            var credentials = ParseBasicAuthenticationScheme(header);

            if (credentials.IsPresent && !credentials.IsMalformed)
            {
                Logger.Debug("Client credential is Basic Authentication");
                return credentials;
            }

            if (credentials.IsMalformed)
            {
                Logger.Warn("Basic Authentication credential is malformed");
                return credentials;
            }

            return ParsePostBody(body);
        }

        public async Task<Client> ValidateClientCredentialsAsync(ClientCredential credential)
        {
            if (credential == null || credential.ClientId == null || credential.Secret == null)
            {
                throw new InvalidOperationException("credential is null");
            }

            var client = await _clients.FindClientByIdAsync(credential.ClientId);
            if (client == null || client.Enabled == false)
            {
                Logger.Error("Client not found in registry or not enabled: " + credential.ClientId);
                return null;
            }

            if (!ObfuscatingComparer.IsEqual(client.ClientSecret, credential.Secret))
            {
                Logger.Error("Invalid client secret: " + client.ClientId);
                return null;
            }

            Logger.InfoFormat("Client found in registry: {0} / {1}", client.ClientId, client.ClientName);
            return client;
        }

        private ClientCredential ParseBasicAuthenticationScheme(AuthenticationHeaderValue header)
        {
            if (header == null ||
                !header.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info("No Basic Authentication header found");

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
                    Secret = secret,

                    IsPresent = true,
                    IsMalformed = false,
                    Type = Constants.ClientAuthenticationMethods.Basic
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
                Logger.Warn("Post body is null.");

                return new ClientCredential
                {
                    IsPresent = false
                };
            }

            var id = body.Get("client_id");
            var secret = body.Get("client_secret");

            if (id.IsPresent() && secret.IsPresent())
            {
                Logger.Debug("Client credentials in POST body found.");

                return new ClientCredential
                {
                    ClientId = id,
                    Secret = secret,

                    IsMalformed = false,
                    IsPresent = true,
                    Type = Constants.ClientAuthenticationMethods.FormPost
                };
            }

            Logger.Debug("No client credentials in POST body found.");
            return new ClientCredential
            {
                IsPresent = false
            };
        }
    }
}