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

namespace IdentityServer3.Tests.Validation.Client_Validation
{
    public class FormPostCredentialExtraction
    {
        const string Category = "Client Credentials - Form Post Credential Extraction";

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var validator = new PostBodyClientValidator(null, null);
            var context = new OwinContext();
            context.Request.Body = new MemoryStream();

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.CredentialType.Should().Be(Constants.ClientCredentialTypes.SharedSecret);
            credential.IsPresent.Should().Be(false);
        }

        [Fact]
        public async void Valid_PostBody()
        {
            var validator = new PostBodyClientValidator(null, null);
            var context = new OwinContext();

            var body = "client_id=client&client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.CredentialType.Should().Be(Constants.ClientCredentialTypes.SharedSecret);
            credential.IsPresent.Should().Be(true);
            credential.ClientId.Should().Be("client");
            credential.Credential.Should().Be("secret");
        }

        [Fact]
        public async void Missing_ClientId()
        {
            var validator = new PostBodyClientValidator(null, null);
            var context = new OwinContext();

            var body = "client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.CredentialType.Should().Be(Constants.ClientCredentialTypes.SharedSecret);
            credential.IsPresent.Should().Be(false);
        }

        [Fact]
        public async void Missing_ClientSecret()
        {
            var validator = new PostBodyClientValidator(null, null);
            var context = new OwinContext();

            var body = "client_id=client";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.CredentialType.Should().Be(Constants.ClientCredentialTypes.SharedSecret);
            credential.IsPresent.Should().Be(false);
        }

        [Fact]
        public async void Malformed_PostBody()
        {
            var validator = new PostBodyClientValidator(null, null);
            var context = new OwinContext();

            var body = "malformed";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var credential = await validator.ExtractCredentialAsync(context.Environment);

            credential.CredentialType.Should().Be(Constants.ClientCredentialTypes.SharedSecret);
            credential.IsPresent.Should().Be(false);
        }
    }
}