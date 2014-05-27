/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ClientValidator
    {
        private readonly IClientService _clients;
        private readonly ILogger _logger;

        public ClientValidator(IClientService clients, ILogger logger)
        {
            _clients = clients;
            _logger = logger;
        }

        public async Task<Client> ValidateClientAsync(NameValueCollection parameters, AuthenticationHeaderValue header)
        {
            // validate client credentials on the wire
            var credential = ValidateHttpRequest(header, parameters);

            if (credential.IsMalformed || !credential.IsPresent)
            {
                _logger.Error("No or malformed client credential found.");
                return null;
            }

            // validate client against configuration store
            var client = await ValidateClientCredentialsAsync(credential);
            if (client == null)
            {
                _logger.Error("Invalid client credentials. Aborting.");
                return null;
            }

            return client;
        }

        public ClientCredential ValidateHttpRequest(AuthenticationHeaderValue header, NameValueCollection body)
        {
            var credentials = ParseBasicAuthenticationScheme(header);

            if (credentials.IsPresent || credentials.IsMalformed)
            {
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
                _logger.Error("Client not found in registry or not enabled: " + credential.ClientId);
                return null;
            }

            if (!ObfuscatingComparer.IsEqual(client.ClientSecret, credential.Secret))
            {
                _logger.Error("Invalid client secret: " + client.ClientId);
                return null;
            }

            _logger.InformationFormat("Client found in registry: {0} / {1}", client.ClientId, client.ClientName);
            return client;
        }

        private ClientCredential ParseBasicAuthenticationScheme(AuthenticationHeaderValue header)
        {
            if (header == null ||
                !header.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Information("No Basic Authentication header found");

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
                _logger.Warning("Post body is null.");

                return new ClientCredential
                {
                    IsPresent = false
                };
            }

            var id = body.Get("client_id");
            var secret = body.Get("client_secret");

            if (id.IsPresent() && secret.IsPresent())
            {
                _logger.Information("Client credentials in POST body found.");

                return new ClientCredential
                {
                    ClientId = id,
                    Secret = secret,

                    IsMalformed = false,
                    IsPresent = true,
                    Type = Constants.ClientAuthenticationMethods.FormPost
                };
            }

            _logger.Information("No client credentials in POST body found.");
            return new ClientCredential
            {
                IsPresent = false
            };
        }
    }
}