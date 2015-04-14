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
using IdentityServer3.Core.Resources;
using IdentityServer3.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xunit;


namespace IdentityServer3.Tests.Conformance.Basic
{
    public class RedirectUriTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.RedirectUriTests";

        Client client;
        string client_id = "code_client";
        string redirect_uri = "https://code_client/callback";
        string client_secret = "secret";

        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);
            host.Clients.Add(client = new Client
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
        public void Reject_redirect_uri_not_matching_registered_redirect_uri()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = host.GetAuthorizeUrl(client_id, "http://bad", "openid", "code", state, nonce);

            var result = host.Client.GetAsync(url).Result;

            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.unauthorized_client);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Reject_request_without_redirect_uri_when_multiple_registered()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = host.GetAuthorizeUrl(client_id, /*redirect_uri*/ null, "openid", "code", state, nonce);

            var result = host.Client.GetAsync(url).Result;

            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.invalid_request);
        }
        
        [Fact]
        [Trait("Category", Category)]
        public void Preserves_query_parameters_in_redirect_uri()
        {
            var query_redirect_uri = redirect_uri + "?foo=bar&baz=quux";
            client.RedirectUris.Add(query_redirect_uri);

            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = host.GetAuthorizeUrl(client_id, query_redirect_uri, "openid", "code", state, nonce);

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Redirect);
            result.Headers.Location.AbsoluteUri.Should().StartWith("https://code_client/callback");
            result.Headers.Location.AbsolutePath.Should().Be("/callback");
            var query = result.Headers.Location.ParseQueryString();
            query.AllKeys.Should().Contain("foo");
            query["foo"].ToString().Should().Be("bar");
            query.AllKeys.Should().Contain("baz");
            query["baz"].ToString().Should().Be("quux");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Rejects_redirect_uri_when_query_parameter_does_not_match()
        {
            var query_redirect_uri = redirect_uri + "?foo=bar&baz=quux";
            client.RedirectUris.Add(query_redirect_uri);
            query_redirect_uri = redirect_uri + "?baz=quux&foo=bar";

            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = host.GetAuthorizeUrl(client_id, query_redirect_uri, "openid", "code", state, nonce);

            var result = host.Client.GetAsync(url).Result;
            
            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.unauthorized_client);
        }
    }
}
