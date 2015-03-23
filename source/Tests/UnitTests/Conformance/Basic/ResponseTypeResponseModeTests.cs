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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Thinktecture.IdentityServer.Core.Models;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Conformance.Basic
{
    public class ResponseTypeResponseModeTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.ResponseTypeResponseModeTests";

        private const string ClientId = "code_client";
        private const string RedirectUri = "https://code_client/callback";
        private const string ClientSecret = "secret";

        protected override void PreInit()
        {
            Host.Scopes.Add(StandardScopes.OpenId);
            Host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = ClientId,
                ClientSecrets = new List<ClientSecret>
                {
                    new ClientSecret(ClientSecret)
                },
                Flow = Flows.AUTHORIZATION_CODE,
                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    RedirectUri
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Request_with_response_type_code_supported()
        {
            Host.Login();
            var cert = Host.GetSigningCertificate();

            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();

            var url = Host.GetAuthorizeUrl(ClientId, RedirectUri, "openid", "code", state, nonce);
            var result = Host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseQueryString();
            query.AllKeys.Should().Contain("code");
            query.AllKeys.Should().Contain("state");
            query["state"].Should().Be(state);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Request_missing_response_type_rejected()
        {
            Host.Login();

            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();

            var url = Host.GetAuthorizeUrl(ClientId, RedirectUri, "openid", /*response_type*/ null, state, nonce);

            var result = Host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);
            result.Headers.Location.AbsoluteUri.Should().Contain("#");

            var query = result.Headers.Location.ParseHashFragment();
            query.AllKeys.Should().Contain("state");
            query["state"].Should().Be(state);
            query.AllKeys.Should().Contain("error");
            query["error"].Should().Be("unsupported_response_type");
        }
    }
}