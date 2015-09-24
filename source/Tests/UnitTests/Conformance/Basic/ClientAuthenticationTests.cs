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
using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xunit;


namespace IdentityServer3.Tests.Conformance.Basic
{
    public class ClientAuthenticationTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.ClientAuthenticationTests";

        string client_id = "code_client";
        string redirect_uri = "https://code_client/callback";
        string client_secret = "secret";

        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);
            
            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.AuthorizationCode,
                AllowAccessToAllScopes = true,
                
                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Token_endpoint_supports_client_authentication_with_basic_authentication_with_POST()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var query = host.RequestAuthorizationCode(client_id, redirect_uri, "openid", nonce);
            var code = query["code"];

            host.NewRequest();

            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(client_id, client_secret);
            
            var result = host.PostForm(host.GetTokenUrl(), 
                new {
                    grant_type="authorization_code", 
                    code, 
                    client_id,
                    redirect_uri 
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
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var query = host.RequestAuthorizationCode(client_id, redirect_uri, "openid", nonce);
            var code = query["code"];

            host.NewRequest();

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    client_id,
                    client_secret,
                    redirect_uri,
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
