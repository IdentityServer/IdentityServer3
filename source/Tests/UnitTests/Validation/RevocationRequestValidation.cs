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
using IdentityServer3.Core;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.Core.Validation;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation
{
    public class RevocationRequestValidation
    {
        const string Category = "Revocation Request Validation Tests";

        TokenRevocationRequestValidator _validator;
        IRefreshTokenStore _refreshTokens;
        ITokenHandleStore _tokenHandles;
        IClientStore _clients;

        public RevocationRequestValidation()
        {
            _refreshTokens = new InMemoryRefreshTokenStore();
            _tokenHandles = new InMemoryTokenHandleStore();
            _clients = new InMemoryClientStore(TestClients.Get());

            _validator = new TokenRevocationRequestValidator();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Parameters()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var parameters = new NameValueCollection();

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_Token_Valid_Hint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            
            var parameters = new NameValueCollection
            {
                { "token_type_hint", "access_token" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_And_AccessTokenHint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
                { "token_type_hint", "access_token" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
            result.Token.Should().Be("foo");
            result.TokenTypeHint.Should().Be("access_token");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_and_RefreshTokenHint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
                { "token_type_hint", "refresh_token" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
            result.Token.Should().Be("foo");
            result.TokenTypeHint.Should().Be("refresh_token");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_And_Missing_Hint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();
            result.Token.Should().Be("foo");
            result.TokenTypeHint.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Token_And_Invalid_Hint()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");

            var parameters = new NameValueCollection
            {
                { "token", "foo" },
                { "token_type_hint", "invalid" }
            };

            var result = await _validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.RevocationErrors.UnsupportedTokenType);
        }
    }
}