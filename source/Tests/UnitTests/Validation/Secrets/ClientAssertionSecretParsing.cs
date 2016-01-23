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
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Validation;
using Microsoft.Owin;
using System.IdentityModel.Tokens;
using System.IO;
using System.Text;
using Xunit;

namespace IdentityServer3.Tests.Validation.Secrets
{
    public class ClientAssertionSecretParsing
    {
        IdentityServerOptions _options;
        ClientAssertionSecretParser _parser;

        public ClientAssertionSecretParsing()
        {
            _options = new IdentityServerOptions();
            _parser = new ClientAssertionSecretParser();
        }

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var context = new OwinContext();
            context.Request.Body = new MemoryStream();

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Valid_ClientAssertion()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=token";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().NotBeNull();
            secret.Type.Should().Be(Constants.ParsedSecretTypes.JwtBearer);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("token");
        }

        [Fact]
        public async void Valid_ClientAssertion_ImplicitClientId()
        {
            var context = new OwinContext();

            var token = new JwtSecurityToken(issuer: "client");
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var body = "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=" + tokenString;

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().NotBeNull();
            secret.Type.Should().Be(Constants.ParsedSecretTypes.JwtBearer);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be(tokenString);
        }

        [Fact]
        public async void Missing_ClientAssertionType()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_assertion=token";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Missing_ClientAssertion()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_assertion_type=urn:ietf:params:oauth:grant-type:jwt-bearer";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Malformed_PostBody()
        {
            var context = new OwinContext();

            var body = "malformed";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }
    }
}
