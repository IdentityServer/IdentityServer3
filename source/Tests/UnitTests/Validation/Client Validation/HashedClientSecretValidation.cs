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

using FluentAssertions;
using System;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using Xunit;

namespace IdentityServer3.Tests.Validation.Client_Validation
{
    public class HashedClientSecretValidation
    {
        const string Category = "Client Validation - Hashed Client Secret Validation";

        IClientSecretValidator _validator = new HashedClientSecretValidator();
        IClientStore _clients = new InMemoryClientStore(ClientValidationTestClients.Get());

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                ClientId = clientId,
                Credential = "secret",
                CredentialType = Constants.ClientCredentialTypes.SharedSecret
            };

            var result = await _validator.ValidateClientSecretAsync(client, credential);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Credential_Type()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                ClientId = clientId,
                Credential = "secret",
                CredentialType = "invalid"
            };

            var result = await _validator.ValidateClientSecretAsync(client, credential);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Multiple_Secrets_No_Protection()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                ClientId = clientId,
                Credential = "secret",
                CredentialType = Constants.ClientCredentialTypes.SharedSecret
            };

            var result = await _validator.ValidateClientSecretAsync(client, credential);
            result.Should().BeTrue();

            credential.Credential = "foobar";
            result = await _validator.ValidateClientSecretAsync(client, credential);
            result.Should().BeTrue();

            credential.Credential = "quux";
            result = await _validator.ValidateClientSecretAsync(client, credential);
            result.Should().BeTrue();

            credential.Credential = "notexpired";
            result = await _validator.ValidateClientSecretAsync(client, credential);
            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                ClientId = clientId,
                Credential = "invalid",
                CredentialType = Constants.ClientCredentialTypes.SharedSecret
            };

            var result = await _validator.ValidateClientSecretAsync(client, credential);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Secret()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                ClientId = clientId,
                Credential = "expired",
                CredentialType = Constants.ClientCredentialTypes.SharedSecret
            };

            var result = await _validator.ValidateClientSecretAsync(client, credential);
            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Multiple_Secrets()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                ClientId = clientId,
                Credential = "invalid",
                CredentialType = Constants.ClientCredentialTypes.SharedSecret
            };

            var result = await _validator.ValidateClientSecretAsync(client, credential);
            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_with_no_Secret_Should_Throw()
        {
            var clientId = "client_no_secret";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                ClientId = clientId,
                CredentialType = Constants.ClientCredentialTypes.SharedSecret
            };

            Func<Task> act = () => _validator.ValidateClientSecretAsync(client, credential);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_with_no_Secret_or_Id_Should_Throw()
        {
            var clientId = "client_no_secret";
            var client = await _clients.FindClientByIdAsync(clientId);

            var credential = new ClientCredential
            {
                CredentialType = Constants.ClientCredentialTypes.SharedSecret
            };

            Func<Task> act = () => _validator.ValidateClientSecretAsync(client, credential);

            act.ShouldThrow<ArgumentNullException>();
        }
    }
}