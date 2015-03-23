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
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.ViewModels;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Conformance.Basic
{
    public class RedirectUriTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.RedirectUriTests";

        Client _client;
        private const string ClientId = "code_client";
        private const string RedirectUri = "https://code_client/callback";
        private const string ClientSecret = "secret";

        protected override void PreInit()
        {
            Host.Scopes.Add(StandardScopes.OpenId);
            Host.Clients.Add(_client = new Client
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
        public void Reject_redirect_uri_not_matching_registered_redirect_uri()
        {
            Host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = Host.GetAuthorizeUrl(ClientId, "http://bad", "openid", "code", state, nonce);

            var result = Host.Client.GetAsync(url).Result;

            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.unauthorized_client);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Reject_request_without_redirect_uri_when_multiple_registered()
        {
            Host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = Host.GetAuthorizeUrl(ClientId, /*redirect_uri*/ null, "openid", "code", state, nonce);

            var result = Host.Client.GetAsync(url).Result;

            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.invalid_request);
        }
        
        [Fact]
        [Trait("Category", Category)]
        public void Preserves_query_parameters_in_redirect_uri()
        {
            const string queryRedirectUri = RedirectUri + "?foo=bar&baz=quux";
            _client.RedirectUris.Add(queryRedirectUri);

            Host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = Host.GetAuthorizeUrl(ClientId, queryRedirectUri, "openid", "code", state, nonce);

            var result = Host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Redirect);
            result.Headers.Location.AbsoluteUri.Should().StartWith("https://code_client/callback");
            result.Headers.Location.AbsolutePath.Should().Be("/callback");
            var query = result.Headers.Location.ParseQueryString();
            query.AllKeys.Should().Contain("foo");
            query["foo"].Should().Be("bar");
            query.AllKeys.Should().Contain("baz");
            query["baz"].Should().Be("quux");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Rejects_redirect_uri_when_query_parameter_does_not_match()
        {
            var queryRedirectUri = RedirectUri + "?foo=bar&baz=quux";
            _client.RedirectUris.Add(queryRedirectUri);
            queryRedirectUri = RedirectUri + "?baz=quux&foo=bar";

            Host.Login();

            var nonce = Guid.NewGuid().ToString();
            var state = Guid.NewGuid().ToString();
            var url = Host.GetAuthorizeUrl(ClientId, queryRedirectUri, "openid", "code", state, nonce);

            var result = Host.Client.GetAsync(url).Result;
            
            result.AssertPage("error");
            var model = result.GetPageModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.unauthorized_client);
        }
    }
}
