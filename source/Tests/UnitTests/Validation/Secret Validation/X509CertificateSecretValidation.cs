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

namespace IdentityServer3.Tests.Validation.Secret_Validation
{
    public class X509CertificateSecretValidation
    {
        const string Category = "Secrets - X.509 Certificate Secret Validation";

        ISecretValidator _thumbprintValidator = new X509CertificateThumbprintSecretValidator();
        IClientStore _clients = new InMemoryClientStore(ClientValidationTestClients.Get());

        [Fact]
        public async Task Valid_Certificate_Thumbprint()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = TestCert.Load(),
                Type = Constants.ParsedSecretTypes.X509Certificate
            };

            var result = await _thumbprintValidator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Invalid_Certificate_Thumbprint()
        {
            var clientId = "certificate_invalid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = TestCert.Load(),
                Type = Constants.ParsedSecretTypes.X509Certificate
            };

            var result = await _thumbprintValidator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Null_Certificate_Should_Throw()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = null,
                Type = Constants.ParsedSecretTypes.X509Certificate
            };

            Func<Task> act = () => _thumbprintValidator.ValidateAsync(client.ClientSecrets, secret);

            act.ShouldThrow<ArgumentException>();
        }
    }
}