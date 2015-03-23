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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.ResponseHandling
{
    public class DefaultRefreshTokenServiceTests : IDisposable
    {
        const string Category = "Default RefreshToken Service";

        readonly IRefreshTokenStore _refreshTokenStore;
        readonly DefaultRefreshTokenService _service;

        readonly Client _roclientAbsoluteRefreshExpirationOneTimeOnly;
        readonly Client _roclientSlidingRefreshExpirationOneTimeOnly;
        readonly Client _roclientAbsoluteRefreshExpirationReuse;

        DateTimeOffset _now;
        public DateTimeOffset UtcNow
        {
            get
            {
                if (_now > DateTimeOffset.MinValue) return _now;
                return DateTimeOffset.UtcNow;
            }
        }

        readonly Func<DateTimeOffset> _originalNowFunc;

        public DefaultRefreshTokenServiceTests()
        {
            _originalNowFunc = DateTimeOffsetHelper.UtcNowFunc;
            DateTimeOffsetHelper.UtcNowFunc = () => UtcNow;

            _roclientAbsoluteRefreshExpirationOneTimeOnly = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_absolute_refresh_expiration_one_time_only",
                ClientSecrets = new List<ClientSecret>
                { 
                    new ClientSecret("secret".Sha256())
                },

                Flow = Flows.RESOURCE_OWNER,

                RefreshTokenExpiration = TokenExpiration.ABSOLUTE,
                RefreshTokenUsage = TokenUsage.ONE_TIME_ONLY,
                AbsoluteRefreshTokenLifetime = 200
            };

            _roclientSlidingRefreshExpirationOneTimeOnly = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_sliding_refresh_expiration_one_time_only",
                ClientSecrets = new List<ClientSecret>
                { 
                    new ClientSecret("secret".Sha256())
                },

                Flow = Flows.RESOURCE_OWNER,

                RefreshTokenExpiration = TokenExpiration.SLIDING,
                RefreshTokenUsage = TokenUsage.ONE_TIME_ONLY,
                AbsoluteRefreshTokenLifetime = 10,
                SlidingRefreshTokenLifetime = 4
            };

            _roclientAbsoluteRefreshExpirationReuse = new Client
            {
                ClientName = "Resource Owner Client",
                Enabled = true,
                ClientId = "roclient_absolute_refresh_expiration_reuse",
                ClientSecrets = new List<ClientSecret>
                { 
                    new ClientSecret("secret".Sha256())
                },

                Flow = Flows.RESOURCE_OWNER,

                RefreshTokenExpiration = TokenExpiration.ABSOLUTE,
                RefreshTokenUsage = TokenUsage.RE_USE,
                AbsoluteRefreshTokenLifetime = 200
            };

            _refreshTokenStore = new InMemoryRefreshTokenStore();
            _service = new DefaultRefreshTokenService(_refreshTokenStore, new DefaultEventService());
        }

        public void Dispose()
        {
            if (_originalNowFunc != null)
            {
                DateTimeOffsetHelper.UtcNowFunc = _originalNowFunc;
            }
        }

        Token CreateAccessToken(Client client, string subjectId, int lifetime, params string[] scopes)
        {
            var claims = new List<Claim> 
            {
                new Claim("client_id", client.ClientId),
                new Claim("sub", subjectId)
            };

            scopes.ToList().ForEach(s => claims.Add(new Claim("scope", s)));

            var token = new Token(Constants.TokenTypes.ACCESS_TOKEN)
            {
                Audience = "https://idsrv3.com/resources",
                Issuer = "https://idsrv3.com",
                Lifetime = lifetime,
                Claims = claims,
                Client = client
            };

            return token;
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Create_Refresh_Token_Absolute_Lifetime()
        {
            var client = _roclientAbsoluteRefreshExpirationOneTimeOnly;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await _service.CreateRefreshTokenAsync(token, client);

            // make sure a handle is returned
            string.IsNullOrWhiteSpace(handle).Should().BeFalse();

            // make sure refresh token is in store
            var refreshToken = await _refreshTokenStore.GetAsync(handle);
            refreshToken.Should().NotBeNull();

            // check refresh token values
            client.ClientId.Should().Be(refreshToken.ClientId);
            client.AbsoluteRefreshTokenLifetime.Should().Be(refreshToken.LifeTime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Create_Refresh_Token_Sliding_Lifetime()
        {
            var client = _roclientSlidingRefreshExpirationOneTimeOnly;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await _service.CreateRefreshTokenAsync(token, client);

            // make sure a handle is returned
            string.IsNullOrWhiteSpace(handle).Should().BeFalse();

            // make sure refresh token is in store
            var refreshToken = await _refreshTokenStore.GetAsync(handle);
            refreshToken.Should().NotBeNull();

            // check refresh token values
            client.ClientId.Should().Be(refreshToken.ClientId);
            client.SlidingRefreshTokenLifetime.Should().Be(refreshToken.LifeTime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Sliding_Expiration_does_not_exceed_absolute_Expiration()
        {
            _now = DateTimeOffset.UtcNow;

            var client = _roclientSlidingRefreshExpirationOneTimeOnly;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await _service.CreateRefreshTokenAsync(token, client);
            var refreshToken = await _refreshTokenStore.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            _now = _now.AddSeconds(8);

            var newHandle = await _service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await _refreshTokenStore.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            newLifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Sliding_Expiration_within_absolute_Expiration()
        {
            _now = DateTimeOffset.UtcNow;

            var client = _roclientSlidingRefreshExpirationOneTimeOnly;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await _service.CreateRefreshTokenAsync(token, client);
            var refreshToken = await _refreshTokenStore.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            _now = _now.AddSeconds(1);

            var newHandle = await _service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await _refreshTokenStore.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            (client.SlidingRefreshTokenLifetime + 1).Should().Be(newLifetime);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task ReUse_Handle_reuses_Handle()
        {
            var client = _roclientAbsoluteRefreshExpirationReuse;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await _service.CreateRefreshTokenAsync(token, client);
            var newHandle = await _service.UpdateRefreshTokenAsync(handle, await _refreshTokenStore.GetAsync(handle), client);

            newHandle.Should().Be(handle);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task OneTime_Handle_creates_new_Handle()
        {
            var client = _roclientAbsoluteRefreshExpirationOneTimeOnly;
            var token = CreateAccessToken(client, "valid", 60, "read", "write");

            var handle = await _service.CreateRefreshTokenAsync(token, client);
            var newHandle = await _service.UpdateRefreshTokenAsync(handle, await _refreshTokenStore.GetAsync(handle), client);

            newHandle.Should().NotBe(handle);
        }
    }
}