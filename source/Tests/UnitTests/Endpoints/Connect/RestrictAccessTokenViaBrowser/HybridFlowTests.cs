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
using IdentityServer3.Tests.Conformance;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace IdentityServer3.Tests.Endpoints.Connect.RestrictAccessTokenViaBrowser
{
    public class HybridFlowTests : IdentityServerHostTest
    {
        const string Category = "Endpoint.Authorize.HybridFlow";

        string client_id = "hybrid";
        string client_id_noBrowser = "hybrid.nobrowser";

        string redirect_uri = "https://hybrid_client/callback";
        string client_secret = "secret";
        string scope = "openid";


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

                Flow = Flows.Hybrid,
                AllowAccessToAllScopes = true,
                AllowAccessTokensViaBrowser = true,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id_noBrowser,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.Hybrid,
                AllowAccessToAllScopes = true,
                AllowAccessTokensViaBrowser = false,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Unrestricted_client_can_request_CodeIdToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#code=");
            result.Headers.Location.AbsoluteUri.Should().Contain("id_token=");
            result.Headers.Location.AbsoluteUri.Should().NotContain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Unrestricted_client_can_request_CodeIdTokenToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#code=");
            result.Headers.Location.AbsoluteUri.Should().Contain("id_token=");
            result.Headers.Location.AbsoluteUri.Should().Contain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_client_can_request_CodeIdToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id_noBrowser,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            result.Headers.Location.AbsoluteUri.Should().StartWith(redirect_uri + "#code=");
            result.Headers.Location.AbsoluteUri.Should().Contain("id_token=");
            result.Headers.Location.AbsoluteUri.Should().NotContain("access_token=");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_client_cannot_request_CodeIdTokenToken()
        {
            host.Login();

            var url = host.GetAuthorizeUrl(
                client_id: client_id_noBrowser,
                redirect_uri: redirect_uri,
                scope: scope,
                response_type: "code id_token token",
                nonce: "nonce");

            var result = host.Client.GetAsync(url).Result;

            // user error page - no protocol response
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}