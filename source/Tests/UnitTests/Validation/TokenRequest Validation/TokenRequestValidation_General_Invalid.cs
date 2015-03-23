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
using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Tests.Validation.Setup;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.TokenRequest_Validation
{
    public class TokenRequestValidation_General_Invalid
    {
        readonly IClientStore _clients = new InMemoryClientStore(TestClients.Get());

        [Fact]
        
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public void Parameters_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            Func<Task> act = () => validator.ValidateRequestAsync(null, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public void Client_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.AUTHORIZATION_CODE},
                {Constants.TokenRequest.CODE, "valid"},
                {Constants.TokenRequest.REDIRECT_URI, "https://server/cb"}
            };

            Func<Task> act = () => validator.ValidateRequestAsync(parameters, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public async Task Unknown_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, "unknown"},
                {Constants.TokenRequest.CODE, "valid"},
                {Constants.TokenRequest.REDIRECT_URI, "https://server/cb"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UNSUPPORTED_GRANT_TYPE);
        }

        [Fact]
        [Trait("Category", "TokenRequest Validation - General - Invalid")]
        public async Task Missing_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.CODE, "valid"},
                {Constants.TokenRequest.REDIRECT_URI, "https://server/cb"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UNSUPPORTED_GRANT_TYPE);
        }
    }
}