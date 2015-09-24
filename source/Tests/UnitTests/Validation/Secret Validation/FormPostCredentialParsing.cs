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
using IdentityServer3.Core.Validation;
using Microsoft.Owin;
using System.IO;
using System.Text;
using Xunit;

namespace IdentityServer3.Tests.Validation.Secret_Validation
{
    public class FormPostCredentialExtraction
    {
        const string Category = "Secrets - Form Post Secret Parsing";

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var parser = new PostBodySecretParser();
            var context = new OwinContext();
            context.Request.Body = new MemoryStream();

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Valid_PostBody()
        {
            var parser = new PostBodySecretParser();
            var context = new OwinContext();

            var body = "client_id=client&client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.SharedSecret);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("secret");
        }

        [Fact]
        public async void Missing_ClientId()
        {
            var parser = new PostBodySecretParser();
            var context = new OwinContext();

            var body = "client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Missing_ClientSecret()
        {
            var parser = new PostBodySecretParser();
            var context = new OwinContext();

            var body = "client_id=client";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Malformed_PostBody()
        {
            var parser = new PostBodySecretParser();
            var context = new OwinContext();

            var body = "malformed";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }
    }
}