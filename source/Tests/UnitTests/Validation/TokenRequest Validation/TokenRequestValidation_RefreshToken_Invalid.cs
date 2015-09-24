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
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.TokenRequest
{

    public class TokenRequestValidation_RefreshToken_Invalid
    {
        const string Category = "TokenRequest Validation - RefreshToken - Invalid";

        IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Non_existing_RefreshToken()
        {
            var store = new InMemoryRefreshTokenStore();
            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, "nonexistent");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_RefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token") { Client = new Client() { ClientId = "roclient" } },
                LifeTime = 10,
                CreationTime = DateTimeOffset.UtcNow.AddSeconds(-15)
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Wrong_Client_Binding_RefreshToken_Request()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Client = new Client
                    {
                        ClientId = "otherclient"
                    },
                    Lifetime = 600,
                    CreationTime = DateTimeOffset.UtcNow
                }
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_has_no_OfflineAccess_Scope_anymore_at_RefreshToken_Request()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Client = new Client
                    {
                        ClientId = "roclient_restricted"
                    },
                },
                LifeTime = 600,
                CreationTime = DateTimeOffset.UtcNow
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient_restricted");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task RefreshToken_Request_with_disabled_User()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(u => u.IsActiveAsync(It.IsAny<IsActiveContext>())).Callback<IsActiveContext>(ctx =>
            {
                ctx.IsActive = false;
            }).Returns(Task.FromResult(0));

            var subjectClaim = new Claim(Constants.ClaimTypes.Subject, "foo");

            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Claims = new List<Claim> { subjectClaim },
                    Client = new Client() { ClientId = "roclient" }
                },
                LifeTime = 600,
                CreationTime = DateTimeOffset.UtcNow
            };
            var handle = Guid.NewGuid().ToString();

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store,
                userService: mock.Object);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
        }
    }
}