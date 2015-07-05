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

using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;

namespace IdentityServer3.Tests.Validation
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

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret")
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

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret")
                    }
                },

                new Client
                {
                    ClientName = "Client with X509 Certificate",
                    ClientId = "certificate_valid",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Type = Constants.SecretTypes.X509CertificateThumbprint,
                            Value = TestCert.Load().Thumbprint
                        }
                    }
                },

                new Client
                {
                    ClientName = "Client with X509 Certificate",
                    ClientId = "certificate_invalid",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Type = Constants.SecretTypes.X509CertificateThumbprint,
                            Value = "invalid"
                        }
                    }
                },

                new Client
                {
                    ClientName = "Client with single secret, hashed, no expiration",
                    ClientId = "single_secret_hashed_no_expiration",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        // secret
                        new Secret("secret".Sha256())
                    }
                },

                new Client
                {
                    ClientName = "Client with multiple secrets, no protection",
                    ClientId = "multiple_secrets_no_protection",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret"),
                        new Secret("foobar", "some description"),
                        new Secret("quux"),
                        new Secret("notexpired", DateTimeOffset.UtcNow.AddDays(1)),
                        new Secret("expired", DateTimeOffset.UtcNow.AddDays(-1)),
                    }
                },

                new Client
                {
                    ClientName = "Client with multiple secrets, hashed",
                    ClientId = "multiple_secrets_hashed",
                    Enabled = true,

                    ClientSecrets = new List<Secret>
                    {
                        // secret
                        new Secret("secret".Sha256()),
                        // foobar
                        new Secret("foobar".Sha256(), "some description"),
                        // quux
                        new Secret("quux".Sha512()),
                        // notexpired
                        new Secret("notexpired".Sha256(), DateTimeOffset.UtcNow.AddDays(1)),
                        // expired
                        new Secret("expired".Sha512(), DateTimeOffset.UtcNow.AddDays(-1)),
                    }
                },
            };
        }
    }
}
