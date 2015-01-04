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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Tests.Connect.Setup
{
    static class ClientValidationTestClients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Disabled client",
                    ClientId = "disabled_client",
                    Enabled = false,

                    ClientSecretProtection = ClientSecretProtection.None,
                    ClientSecrets = new List<ClientSecret>
                    {
                        new ClientSecret("secret")
                    }
                },

                new Client
                {
                    ClientName = "Client with no secret set",
                    ClientId = "no_secret_client",
                    Enabled = true
                },

                new Client
                {
                    ClientName = "Client with single secret, no protection, no expiration",
                    ClientId = "single_secret_no_protection_no_expiration",
                    Enabled = true,

                    ClientSecretProtection = ClientSecretProtection.None,
                    ClientSecrets = new List<ClientSecret>
                    {
                        new ClientSecret("secret")
                    }
                },

                new Client
                {
                    ClientName = "Client with single secret, hashed, no expiration",
                    ClientId = "single_secret_hashed_no_expiration",
                    Enabled = true,

                    ClientSecretProtection = ClientSecretProtection.Hashed,
                    ClientSecrets = new List<ClientSecret>
                    {
                        // secret
                        new ClientSecret("K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=")
                    }
                },

                new Client
                {
                    ClientName = "Client with multiple secrets, no protection",
                    ClientId = "multiple_secrets_no_protection",
                    Enabled = true,

                    ClientSecretProtection = ClientSecretProtection.None,
                    ClientSecrets = new List<ClientSecret>
                    {
                        new ClientSecret("secret"),
                        new ClientSecret("foobar"),
                        new ClientSecret("quux")
                    }
                },

                new Client
                {
                    ClientName = "Client with multiple secrets, hashed",
                    ClientId = "multiple_secrets_hashed",
                    Enabled = true,

                    ClientSecretProtection = ClientSecretProtection.Hashed,
                    ClientSecrets = new List<ClientSecret>
                    {
                        // secret
                        new ClientSecret("K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols="),
                        // foobar
                        new ClientSecret("w6uP8Tcg6K2QR905Rms8iXTlksL6OD1KOWBxTK7wxPI="),
                        // quux
                        new ClientSecret("BTBX/ampNfLU+ox7xipBGiaSbgC0kcB8Gy7BkJB4oKI=")
                    }
                },
            };
        }
    }
}
