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
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IdentityServer3.Tests.Validation.Secrets
{
    public class BasicAuthenticationSecretParsing
    {
        const string Category = "Secrets - Basic Authentication Secret Parsing";
        IdentityServerOptions _options;
        BasicAuthenticationSecretParser _parser;

        public BasicAuthenticationSecretParsing()
        {
            _options = new IdentityServerOptions();
            _parser = new BasicAuthenticationSecretParser(_options);
        }

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var context = new OwinContext();
            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request()
        {
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:secret")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.SharedSecret);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic" }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request_ClientId_Too_Long()
        {
            var context = new OwinContext();

            var longClientId = "x".Repeat(_options.InputLengthRestrictions.ClientId + 1);
            var credential = string.Format("{0}:secret", longClientId);

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(credential)));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);
            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request_ClientSecret_Too_Long()
        {
            var context = new OwinContext();
            
            var longClientSecret = "x".Repeat(_options.InputLengthRestrictions.ClientSecret + 1);
            var credential = string.Format("client:{0}", longClientSecret);

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(credential)));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);
            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header_Variation()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic " }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Unknown_Scheme()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Unknown" }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_NoBase64_Encoding()
        {
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic somerandomdata" }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only()
        {
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only_With_Colon()
        {
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }
    }
}