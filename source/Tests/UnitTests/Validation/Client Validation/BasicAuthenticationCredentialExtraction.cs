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
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Text;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.Client_Validation
{
    public class BasicAuthenticationCredentialExtraction
    {
        const string Category = "Client Credentials - Basic Authentication Credential Extraction";

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.CredentialType.Should().Be(Constants.ClientCredentialTypes.SharedSecret);
            credential.IsPresent.Should().Be(false);
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:secret")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.CredentialType.Should().Be(Constants.ClientCredentialTypes.SharedSecret);
            credential.IsPresent.Should().Be(true);
            credential.ClientId.Should().Be("client");
            credential.Credential.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic" }));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header_Variation()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic " }));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Unknown_Scheme()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Unknown" }));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_NoBase64_Encoding()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { "Basic somerandomdata" }));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only_With_Colon()
        {
            var validator = new BasicAuthenticationClientValidator(null, null);
            var context = new OwinContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:")));
            context.Request.Headers.Add(
                new KeyValuePair<string, string[]>("Authorization", new[] { headerValue }));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.IsPresent.Should().BeFalse();
        }
    }
}