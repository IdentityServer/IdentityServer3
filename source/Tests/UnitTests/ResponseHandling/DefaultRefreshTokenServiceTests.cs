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
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Tests.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Connect.ResponseHandling
{
    public class DefaultRefreshTokenServiceTests
    {
        const string Category = "Default RefreshToken Service";

        IRefreshTokenStore refreshTokenStore;
        DefaultRefreshTokenService service;

        Client roclient_absolute_refresh_expiration_one_time_only;
        Client roclient_sliding_refresh_expiration_one_time_only;
        Client roclient_absolute_refresh_expiration_reuse;

        public DefaultRefreshTokenServiceTests()
        {
            roclient_absolute_refresh_expiration_one_time_only = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_absolute_refresh_expiration_one_time_only",
                ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                Flow = Flows.ResourceOwner,

                RefreshTokenExpiration = TokenExpiration.Absolute,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                AbsoluteRefreshTokenLifetime = 200
            };

            roclient_sliding_refresh_expiration_one_time_only = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_sliding_refresh_expiration_one_time_only",
                ClientSecrets = new List<ClientSecret>
                { 
                    new ClientSecret("secret".Sha256())
                },

                Flow = Flows.ResourceOwner,

                RefreshTokenExpiration = TokenExpiration.Sliding,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                AbsoluteRefreshTokenLifetime = 10,
                SlidingRefreshTokenLifetime = 4
            };

            roclient_absolute_refresh_expiration_reuse = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_absolute_refresh_expiration_reuse",
                ClientSecrets = new List<ClientSecret>
                        { 
                            new ClientSecret("secret".Sha256())
                        },

                Flow = Flows.ResourceOwner,

                RefreshTokenExpiration = TokenExpiration.Absolute,
                RefreshTokenUsage = TokenUsage.ReUse,
                AbsoluteRefreshTokenLifetime = 200
            };

            refreshTokenStore = new InMemoryRefreshTokenStore();
            service = new DefaultRefreshTokenService(refreshTokenStore, new DefaultEventService());
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Create_Refresh_Token_Absolute_Lifetime()
        {
            var client = roclient_absolute_refresh_expiration_one_time_only;
            var token = TokenFactory.CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);

            // make sure a handle is returned
            string.IsNullOrWhiteSpace(handle).Should().BeFalse();

            // make sure refresh token is in store
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            refreshToken.Should().NotBeNull();

            // check refresh token values
            client.ClientId.Should().Be(refreshToken.ClientId);
            client.AbsoluteRefreshTokenLifetime.Should().Be(refreshToken.LifeTime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Create_Refresh_Token_Sliding_Lifetime()
        {
            var client = roclient_sliding_refresh_expiration_one_time_only;
            var token = TokenFactory.CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);

            // make sure a handle is returned
            string.IsNullOrWhiteSpace(handle).Should().BeFalse();

            // make sure refresh token is in store
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            refreshToken.Should().NotBeNull();

            // check refresh token values
            client.ClientId.Should().Be(refreshToken.ClientId);
            client.SlidingRefreshTokenLifetime.Should().Be(refreshToken.LifeTime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Sliding_Expiration_does_not_exceed_absolute_Expiration()
        {
            var client = roclient_sliding_refresh_expiration_one_time_only;
            var token = TokenFactory.CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            await Task.Delay(8000);

            var newHandle = await service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await refreshTokenStore.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            newLifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Sliding_Expiration_within_absolute_Expiration()
        {
            var client = roclient_sliding_refresh_expiration_one_time_only;
            var token = TokenFactory.CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var refreshToken = await refreshTokenStore.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            await Task.Delay(1000);

            var newHandle = await service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await refreshTokenStore.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            (client.SlidingRefreshTokenLifetime + 1).Should().Be(newLifetime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ReUse_Handle_reuses_Handle()
        {
            var client = roclient_absolute_refresh_expiration_reuse;
            var token = TokenFactory.CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var newHandle = await service.UpdateRefreshTokenAsync(handle, await refreshTokenStore.GetAsync(handle), client);

            newHandle.Should().Be(handle);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OneTime_Handle_creates_new_Handle()
        {
            var client = roclient_absolute_refresh_expiration_one_time_only;
            var token = TokenFactory.CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var newHandle = await service.UpdateRefreshTokenAsync(handle, await refreshTokenStore.GetAsync(handle), client);

            newHandle.Should().NotBe(handle);
        }
    }
}