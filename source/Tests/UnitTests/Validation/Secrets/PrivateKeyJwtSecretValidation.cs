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
using IdentityModel;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.Core.Validation;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.Secrets
{
    public class PrivateKeyJwtSecretValidation
    {
        readonly ISecretValidator _validator;
        readonly IClientStore _clients;

        public PrivateKeyJwtSecretValidation()
        {
            _validator = new PrivateKeyJwtSecretValidator(
                    new IdentityServerOptions()
                    {
                        DynamicallyCalculatedIssuerUri = "https://idsrv3.com"
                    }
                );
            _clients = new InMemoryClientStore(ClientValidationTestClients.Get());
        }

        private JwtSecurityToken CreateToken(string clientId, DateTime? nowOverride = null)
        {
            var certificate = TestCert.Load();
            var now = nowOverride ?? DateTime.Now;

            var token = new JwtSecurityToken(
                    clientId,
                    "https://idsrv3.com/connect/token",
                    new List<Claim>()
                    {
                        new Claim("jti", Guid.NewGuid().ToString()),
                        new Claim(JwtClaimTypes.Subject, clientId),
                        new Claim(JwtClaimTypes.IssuedAt, UnixTime(now).ToString(), ClaimValueTypes.Integer64)
                    },
                    now,
                    now.AddMinutes(1),
                    new X509SigningCredentials(certificate,
                                               SecurityAlgorithms.RsaSha256Signature,
                                               SecurityAlgorithms.Sha256Digest)
                );
            var rawCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
            token.Header.Add(JwtHeaderParameterNames.X5c, new[] { rawCertificate });
            return token;
        }

        private int UnixTime(DateTimeOffset dateTime)
        {
            return (int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        [Fact]
        public async Task Valid_Certificate_Thumbprint()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Valid_Certificate_X5c_Only()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove(JwtHeaderParameterNames.X5t);
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Valid_Certificate_X5t_Only()
        {
            var clientId = "certificate_base64_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove(JwtHeaderParameterNames.X5c);
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Invalid_Certificate_X5t_Only_Requires_Full_Certificate()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove(JwtHeaderParameterNames.X5c);
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Certificate_Thumbprint()
        {
            var clientId = "certificate_invalid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Valid_Certificate_Base64()
        {
            var clientId = "certificate_base64_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Invalid_Certificate_Base64()
        {
            var clientId = "certificate_base64_invalid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(CreateToken(clientId)),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Issuer()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Payload.Remove(JwtClaimTypes.Issuer);
            token.Payload.Add(JwtClaimTypes.Issuer, "invalid");
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Subject()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Payload.Remove(JwtClaimTypes.Subject);
            token.Payload.Add(JwtClaimTypes.Subject, "invalid");
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Expired_Token()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId, DateTime.Now.AddHours(-1));
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Invalid_Unsigned_Token()
        {
            var clientId = "certificate_valid";
            var client = await _clients.FindClientByIdAsync(clientId);

            var token = CreateToken(clientId);
            token.Header.Remove("alg");
            token.Header.Add("alg", "none");
            var secret = new ParsedSecret
            {
                Id = clientId,
                Credential = new JwtSecurityTokenHandler().WriteToken(token),
                Type = Constants.ParsedSecretTypes.JwtBearer
            };

            var result = await _validator.ValidateAsync(client.ClientSecrets, secret);

            result.Success.Should().BeFalse();
        }
    }
}
