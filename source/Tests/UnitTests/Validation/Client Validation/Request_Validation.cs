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
using System;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using Thinktecture.IdentityModel.Http;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.Clients
{
    public class Request_Validation
    {
        const string Category = "Client Credentials - Request Validation";

        ClientValidator _validator = Factory.CreateClientValidator();

        [Fact]
        [Trait("Category", Category)]
        public void Valid_BasicAuthentication_Request()
        {
            var header = new BasicAuthenticationHeaderValue("client", "secret");

            var credential = _validator.ValidateHttpRequest(header, null);

            credential.IsMalformed.Should().BeFalse();
            credential.IsPresent.Should().BeTrue();
            credential.Type.Should().Be(Constants.ClientAuthenticationMethods.Basic);

            credential.ClientId.Should().Be("client");
            credential.Secret.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Valid_FormPost_Request()
        {
            var body = new NameValueCollection();
            body.Add("client_id", "client");
            body.Add("client_secret", "secret");

            var credential = _validator.ValidateHttpRequest(null, body);

            credential.IsMalformed.Should().BeFalse();
            credential.IsPresent.Should().BeTrue();
            credential.Type.Should().Be(Constants.ClientAuthenticationMethods.FormPost);

            credential.ClientId.Should().Be("client");
            credential.Secret.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Valid_BasicAuthentication_and_FormPost_Request()
        {
            var header = new BasicAuthenticationHeaderValue("client", "secret");

            var body = new NameValueCollection();
            body.Add("client_id", "client");
            body.Add("client_secret", "secret");

            var credential = _validator.ValidateHttpRequest(header, null);

            credential.IsMalformed.Should().BeFalse();
            credential.IsPresent.Should().BeTrue();
            credential.Type.Should().Be(Constants.ClientAuthenticationMethods.Basic);

            credential.ClientId.Should().Be("client");
            credential.Secret.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public void No_Client_Credentials()
        {
            var credential = _validator.ValidateHttpRequest(null, null);

            credential.IsMalformed.Should().BeFalse();
            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void BasicAuthentication_Request_With_Empty_Basic_Header()
        {
            var header = new AuthenticationHeaderValue("Basic");

            var credential = _validator.ValidateHttpRequest(header, null);

            credential.IsMalformed.Should().BeTrue();
            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void BasicAuthentication_Request_With_Unknown_Scheme()
        {
            var header = new AuthenticationHeaderValue("Unkown", "data");

            var credential = _validator.ValidateHttpRequest(header, null);

            credential.IsMalformed.Should().BeFalse();
            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void BasicAuthentication_Request_With_Malformed_Credentials_NoBase64_Encoding()
        {
            var header = new AuthenticationHeaderValue("Basic", "somerandomdata");

            var credential = _validator.ValidateHttpRequest(header, null);

            credential.IsMalformed.Should().BeTrue();
            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only()
        {
            var invalidCred = "username";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidCred));
            var header = new AuthenticationHeaderValue("Basic", encoded);

            var credential = _validator.ValidateHttpRequest(header, null);

            credential.IsMalformed.Should().BeTrue();
            credential.IsPresent.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only_With_Colon()
        {
            var invalidCred = "username:";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidCred));
            var header = new AuthenticationHeaderValue("Basic", encoded);

            var credential = _validator.ValidateHttpRequest(header, null);

            credential.IsMalformed.Should().BeTrue();
            credential.IsPresent.Should().BeFalse();
        }
    }
}