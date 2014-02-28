using System;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ClientValidator
    {
        private ICoreSettings _settings;
        private ILogger _logger;

        public ClientValidator(ICoreSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public ClientCredential ValidateRequest(AuthenticationHeaderValue header, NameValueCollection body)
        {
            var credentials = ParseBasicAuthenticationScheme(header);

            if (credentials.IsPresent || credentials.IsMalformed)
            {
                return credentials;
            }

            return ParsePostBody(body);
        }

        public Client ValidateClient(ClientCredential credential)
        {
            if (credential == null || credential.ClientId == null || credential.Secret == null)
            {
                throw new InvalidOperationException("credential is null");
            }

            var client = _settings.FindClientById(credential.ClientId);
            if (client == null)
            {
                _logger.Error("Client not found in registry: " + credential.ClientId);
                return null;
            }

            if (client.ClientSecret != credential.Secret)
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