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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Xunit;
using System.Net.Http;
using Thinktecture.IdentityModel.Http;
using Thinktecture.IdentityServer.Core.ViewModels;
using Thinktecture.IdentityServer.Core.Resources;


namespace Thinktecture.IdentityServer.Tests.Conformance.Basic
{
    public class RedirectUriTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.RedirectUriTests";

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
                ClientSecrets = new List<ClientSecret>
                {
                    new ClientSecret(client_secret)
                },
                Flow = Flows.AuthorizationCode,
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
    }
}
