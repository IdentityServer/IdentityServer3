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
    public class ResponseTypeResponseModeTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.ResponseTypeResponseModeTests";

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
                    new Secret(client_secret)
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
        public void Request_with_response_type_code_supported()
        {
            host.Login();
            var cert = host.GetSigningCertificate();

            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();

            var url = host.GetAuthorizeUrl(client_id, redirect_uri, "openid", "code", state, nonce);
            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseQueryString();
            query.AllKeys.Should().Contain("code");
            query.AllKeys.Should().Contain("state");
            query["state"].Should().Be(state);
        }

        // todo brock: update to new behavior
        //[Fact]
        //[Trait("Category", Category)]
        //public void Request_missing_response_type_rejected()
        //{
        //    host.Login();

        //    var state = Guid.NewGuid().ToString();
        //    var nonce = Guid.NewGuid().ToString();

        //    var url = host.GetAuthorizeUrl(client_id, redirect_uri, "openid", /*response_type*/ null, state, nonce);

        //    var result = host.Client.GetAsync(url).Result;
        //    result.StatusCode.Should().Be(HttpStatusCode.Found);
        //    result.Headers.Location.AbsoluteUri.Should().Contain("#");

        //    var query = result.Headers.Location.ParseHashFragment();
        //    query.AllKeys.Should().Contain("state");
        //    query["state"].Should().Be(state);
        //    query.AllKeys.Should().Contain("error");
        //    query["error"].Should().Be("unsupported_response_type");
        //}
    }
}
