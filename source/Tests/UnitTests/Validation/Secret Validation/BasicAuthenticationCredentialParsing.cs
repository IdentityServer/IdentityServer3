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
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IdentityServer3.Tests.Validation.Secret_Validation
{
    public class BasicAuthenticationSecretParsing
    {
        const string Category = "Secrets - Basic Authentication Secret Parsing";

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:secret")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.SharedSecret);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic" }));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header_Variation()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic " }));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Unknown_Scheme()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Unknown" }));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_NoBase64_Encoding()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic somerandomdata" }));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only_With_Colon()
        {
            var parser = new BasicAuthenticationSecretParser();
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }
    }
}