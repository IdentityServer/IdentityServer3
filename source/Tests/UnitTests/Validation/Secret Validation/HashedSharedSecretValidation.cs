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
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.Core.Validation;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.Client_Validation
{
    public class HashedSharedSecretValidation
    {
        const string Category = "Secrets - Hashed Shared Secret Validation";

        ISecretValidator _validator = new HashedSharedSecretValidator();
        IClientStore _clients = new InMemoryClientStore(ClientValidationTestClients.Get());

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Credential_Type()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = "invalid"
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Multiple_Secrets()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "secret",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);
            result.Success.Should().BeTrue();

            secret.Credential = "foobar";
            result = await _validator.ValidateAsync(client.ClientSecrets, secret);
            result.Success.Should().BeTrue();

            secret.Credential = "quux";
            result = await _validator.ValidateAsync(client.ClientSecrets, secret);
            result.Success.Should().BeTrue();

            secret.Credential = "notexpired";
            result = await _validator.ValidateAsync(client.ClientSecrets, secret);
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Single_Secret()
        {
            var clientId = "single_secret_hashed_no_expiration";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "invalid",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Secret()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "expired",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Multiple_Secrets()
        {
            var clientId = "multiple_secrets_hashed";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = "invalid",
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);
            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_with_no_Secret_Should_Throw()
        {
            var clientId = "no_secret_client";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Type = Constants.ParsedSecretTypes.SharedSecret
            };

            Func<Task> act = () => _validator.ValidateAsync(client.ClientSecrets, secret);

            act.ShouldThrow<ArgumentNullException>();
        }
    }
}