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


namespace Thinktecture.IdentityServer.Tests.Conformance.Basic
{
    public class ResponseTypeResponseModeTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.ResponseTypeResponseModeTests";

        protected override void PreInit()
        {
            host.Scopes.Add(new Scope
            {
                Enabled = true,
                Name = "test_scope",
                Type = ScopeType.Resource,
            });

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = "code_client",
                ClientSecrets = new List<ClientSecret>{
                    new ClientSecret("secret".Sha256())
                },
                Flow = Flows.AuthorizationCode,
                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    "https://code_client/callback"
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Request_with_response_type_code_supported()
        {
            host.Login();

            var state = Guid.NewGuid().ToString();

            var url = host.GetAuthorizeUrl() 
                + "?state=" + state + "&response_type=code&scope=test_scope&client_id=code_client&redirect_uri=https://code_client/callback";
            
            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseQueryString();
            query.AllKeys.Should().Contain("code");
            query.AllKeys.Should().Contain("state");
            query["state"].Should().Be(state);

            host.NewRequest();

            var code = query["code"];
            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue("code_client", "secret");
            result = host.PostForm(host.GetTokenUrl(), 
                new {
                    grant_type="authorization_code", 
                    code=code, 
                    client_id="code_client",
                    redirect_uri="https://code_client/callback" 
                }
            );
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var data = result.ReadJsonObject();
            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("Bearer");
            data["access_token"].Should().NotBeNull();
        }
        
        [Fact]
        [Trait("Category", Category)]
        public void Request_missing_response_type_rejected()
        {
            host.Login();

            var state = Guid.NewGuid().ToString();
            
            var url = host.GetAuthorizeUrl() + 
                "?state=" + state + "&scope=test_scope&client_id=code_client&redirect_uri=https://code_client/callback";

            var result = host.Client.GetAsync(url).Result;
            result.StatusCode.Should().Be(HttpStatusCode.Found);
            result.Headers.Location.AbsoluteUri.Should().Contain("#");

            var query = result.Headers.Location.ParseHashFragment();
            //query.AllKeys.Should().Contain("state");
            //query["state"].Should().Be(state);
            query.AllKeys.Should().Contain("error");
            query["error"].Should().Be("unsupported_response_type");
        }
    }
}
