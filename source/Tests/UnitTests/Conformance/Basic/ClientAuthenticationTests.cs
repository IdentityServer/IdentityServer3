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
using FluentAssertions;
using Thinktecture.IdentityModel.Http;
using Thinktecture.IdentityServer.Core.Models;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Conformance.Basic
{
    public class ClientAuthenticationTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.ClientAuthenticationTests";

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
        public void Token_endpoint_supports_client_authentication_with_basic_authentication_with_POST()
        {
            Host.Login();

            var nonce = Guid.NewGuid().ToString();
            var query = Host.RequestAuthorizationCode(ClientId, RedirectUri, "openid", nonce);
            var code = query["code"];

            Host.NewRequest();

            Host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(ClientId, ClientSecret);
            
            var result = Host.PostForm(Host.GetTokenUrl(), 
                new {
                    grant_type="authorization_code", 
                    code, 
                    client_id = ClientId,
                    redirect_uri = RedirectUri 
                }
            );
            
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.CacheControl.NoCache.Should().BeTrue();
            result.Headers.CacheControl.NoStore.Should().BeTrue();
            
            var data = result.ReadJsonObject();
            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("Bearer");
            data["access_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            data["id_token"].Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Token_endpoint_supports_client_authentication_with_form_encoded_authentication_in_POST_body()
        {
            Host.Login();

            var nonce = Guid.NewGuid().ToString();
            var query = Host.RequestAuthorizationCode(ClientId, RedirectUri, "openid", nonce);
            var code = query["code"];

            Host.NewRequest();

            var result = Host.PostForm(Host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    client_id = ClientId,
                    client_secret = ClientSecret,
                    redirect_uri = RedirectUri,
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.CacheControl.NoCache.Should().BeTrue();
            result.Headers.CacheControl.NoStore.Should().BeTrue();

            var data = result.ReadJsonObject();
            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("Bearer");
            data["access_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            data["id_token"].Should().NotBeNull();
        }
    }
}
