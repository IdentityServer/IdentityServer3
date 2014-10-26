/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.ResponseHandling
{
    [TestClass]
    public class DefaultRefreshTokenServiceTests
    {
        const string Category = "Default RefreshToken Service";

        IClientStore _clients = Factory.CreateClientStore();

        [TestMethod]
        [TestCategory(Category)]
        public async Task Create_Refresh_Token_Absolute_Lifetime()
        {
            var store = new InMemoryRefreshTokenStore();
            var service = new DefaultRefreshTokenService(store);

            var client = await _clients.FindClientByIdAsync("roclient_absolute_refresh_expiration_one_time_only");
            var token = TokenFactory.CreateAccessToken(client.ClientId, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);

            // make sure a handle is returned
            Assert.IsFalse(string.IsNullOrWhiteSpace(handle));

            // make sure refresh token is in store
            var refreshToken = await store.GetAsync(handle);
            Assert.IsNotNull(refreshToken);

            // check refresh token values
            Assert.AreEqual(refreshToken.ClientId, client.ClientId);
            Assert.AreEqual(refreshToken.LifeTime, client.AbsoluteRefreshTokenLifetime);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Create_Refresh_Token_Sliding_Lifetime()
        {
            var store = new InMemoryRefreshTokenStore();
            var service = new DefaultRefreshTokenService(store);

            var client = await _clients.FindClientByIdAsync("roclient_sliding_refresh_expiration_one_time_only");
            var token = TokenFactory.CreateAccessToken(client.ClientId, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);

            // make sure a handle is returned
            Assert.IsFalse(string.IsNullOrWhiteSpace(handle));

            // make sure refresh token is in store
            var refreshToken = await store.GetAsync(handle);
            Assert.IsNotNull(refreshToken);

            // check refresh token values
            Assert.AreEqual(refreshToken.ClientId, client.ClientId);
            Assert.AreEqual(refreshToken.LifeTime, client.SlidingRefreshTokenLifetime);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Sliding_Expiration_does_not_exceed_absolute_Expiration()
        {
            var store = new InMemoryRefreshTokenStore();
            var service = new DefaultRefreshTokenService(store);

            var client = await _clients.FindClientByIdAsync("roclient_sliding_refresh_expiration_one_time_only");
            var token = TokenFactory.CreateAccessToken(client.ClientId, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var refreshToken = await store.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            await Task.Delay(8000);

            var newHandle = await service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await store.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            Assert.AreEqual(client.AbsoluteRefreshTokenLifetime, newLifetime);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Sliding_Expiration_within_absolute_Expiration()
        {
            var store = new InMemoryRefreshTokenStore();
            var service = new DefaultRefreshTokenService(store);

            var client = await _clients.FindClientByIdAsync("roclient_sliding_refresh_expiration_one_time_only");
            var token = TokenFactory.CreateAccessToken(client.ClientId, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var refreshToken = await store.GetAsync(handle);
            var lifetime = refreshToken.LifeTime;

            await Task.Delay(1000);

            var newHandle = await service.UpdateRefreshTokenAsync(handle, refreshToken, client);
            var newRefreshToken = await store.GetAsync(newHandle);
            var newLifetime = newRefreshToken.LifeTime;

            Assert.AreEqual(newLifetime, client.SlidingRefreshTokenLifetime + 1);

        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task ReUse_Handle_reuses_Handle()
        {
            var store = new InMemoryRefreshTokenStore();
            var service = new DefaultRefreshTokenService(store);

            var client = await _clients.FindClientByIdAsync("roclient_absolute_refresh_expiration_reuse");
            var token = TokenFactory.CreateAccessToken(client.ClientId, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var newHandle = await service.UpdateRefreshTokenAsync(handle, await store.GetAsync(handle), client);

            Assert.AreEqual(handle, newHandle);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task OneTime_Handle_creates_new_Handle()
        {
            var store = new InMemoryRefreshTokenStore();
            var service = new DefaultRefreshTokenService(store);

            var client = await _clients.FindClientByIdAsync("roclient_absolute_refresh_expiration_one_time_only");
            var token = TokenFactory.CreateAccessToken(client.ClientId, "valid", 60, "read", "write");

            var handle = await service.CreateRefreshTokenAsync(token, client);
            var newHandle = await service.UpdateRefreshTokenAsync(handle, await store.GetAsync(handle), client);

            Assert.AreNotEqual(handle, newHandle);
        }

        private async Task<ValidatedTokenRequest> CreateValidatedRequest(Client client, IRefreshTokenStore store)
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token"),
                ClientId = client.ClientId,
                LifeTime = 600,
                CreationTime = DateTime.UtcNow
            };
            var handle = Guid.NewGuid().ToString();

            await store.StoreAsync(handle, refreshToken);

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
            return validator.ValidatedRequest;
        }
    }
}