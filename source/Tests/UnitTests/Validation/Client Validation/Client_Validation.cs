﻿/*
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
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.Clients
{
    public class Client_Validation
    {
        ClientValidator _validatorHashed = Factory.CreateClientValidator(
            secretValidator: new HashedClientSecretValidator());
        ClientValidator _validatorPlain = Factory.CreateClientValidator(
            secretValidator: new PlainTextClientSecretValidator());

        const string Category = "Client validation";

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Single_Secret_No_Protection()
        {
            var clientId = "single_secret_no_protection_no_expiration";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "secret",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Single_Secret_Hashed()
        {
            var clientId = "single_secret_hashed_no_expiration";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "secret",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Multiple_Secrets_No_Protection()
        {
            var clientId = "multiple_secrets_no_protection";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "secret",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);

            credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "foobar",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);

            credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "quux",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);

            credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "notexpired",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Multiple_Secrets_Hashed()
        {
            var clientId = "multiple_secrets_hashed";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "secret",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.FormPost
            };

            var client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);

            credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "foobar",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);

            credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "quux",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.FormPost
            };

            client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);


            credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "notexpired",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.FormPost
            };

            client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().NotBeNull();
            client.ClientId.Should().Be(clientId);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Single_Secret_No_Protection()
        {
            var clientId = "single_secret_no_protection_no_expiration";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "invalid",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Secret_No_Protection()
        {
            var clientId = "multiple_secrets_no_protection";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "expired",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Multiple_Secrets_No_Protection()
        {
            var clientId = "multiple_secrets_no_protection";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "invalid",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorPlain.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Multiple_Secrets_Hashed()
        {
            var clientId = "multiple_secrets_hashed";

            var credential = new ClientCredential
            {
                ClientId = clientId,
                SharedSecret = "invalid",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unkown_Client()
        {
            var credential = new ClientCredential
            {
                ClientId = "unknown",
                SharedSecret = "invalid",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Disabled_Client()
        {
            var credential = new ClientCredential
            {
                ClientId = "disabled_client",
                SharedSecret = "secret",
                AuthenticationMethod = ClientAuthenticationMethods.Basic,
                IsPresent = true
            };

            var client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        
        [Trait("Category", Category)]
        public void Null_Client_Credentials()
        {
            var credential = new ClientCredential();

            Func<Task> act = () => _validatorHashed.ValidateClientCredentialsAsync(credential);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        
        [Trait("Category", Category)]
        public void Null_ClientId()
        {
            var credential = new ClientCredential();

            Func<Task> act = () => _validatorHashed.ValidateClientCredentialsAsync(credential);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Client_Credentials()
        {
            var credential = new ClientCredential
            {
                ClientId = "",
                SharedSecret = "",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Empty_Client_Credentials_Not_Present()
        {
            var credential = new ClientCredential
            {
                ClientId = "",
                SharedSecret = "",
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            Func<Task> act = () => _validatorHashed.ValidateClientCredentialsAsync(credential);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Secret_Client_Credentials_Empty_Secret()
        {
            var credential = new ClientCredential
            {
                ClientId = "no_secret_client",
                SharedSecret = "",
                IsPresent = true,
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            var client = await _validatorHashed.ValidateClientCredentialsAsync(credential);

            client.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void No_Secret_Client_Credentials_No_Secret()
        {
            var credential = new ClientCredential
            {
                ClientId = "no_secret_client",
                AuthenticationMethod = ClientAuthenticationMethods.Basic
            };

            Func<Task> act = () => _validatorHashed.ValidateClientCredentialsAsync(credential);

            act.ShouldThrow<InvalidOperationException>();
        }
    }
}