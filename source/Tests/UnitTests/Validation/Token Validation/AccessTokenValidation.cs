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
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.Tokens
{
    public class AccessTokenValidation : IDisposable
    {
        const string Category = "Access token validation";

        IClientStore _clients = Factory.CreateClientStore();

        static AccessTokenValidation()
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
        }

        DateTimeOffset now;
        public DateTimeOffset UtcNow
        {
            get
            {
                if (now > DateTimeOffset.MinValue) return now;
                return DateTimeOffset.UtcNow;
            }
        }

        Func<DateTimeOffset> originalNowFunc;
        
        public AccessTokenValidation()
        {
            originalNowFunc = DateTimeOffsetHelper.UtcNowFunc;
            DateTimeOffsetHelper.UtcNowFunc = () => UtcNow;
        }

        public void Dispose()
        {
            if (originalNowFunc != null)
            {
                DateTimeOffsetHelper.UtcNowFunc = originalNowFunc;
            }
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeFalse();
            result.Claims.Count().Should().Be(8);
            result.Claims.First(c => c.Type == Constants.ClaimTypes.ClientId).Value.Should().Be("roclient");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_required_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "read");

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_missing_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "missing");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InsufficientScope);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unknown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Reference_Token()
        {
            now = DateTimeOffset.UtcNow;

            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 2, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);
            
            now = now.AddMilliseconds(2000);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.ExpiredToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Malformed_JWT_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unk.nown");

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_JWT_Token()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write"));

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Issuer()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Issuer = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Audience()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "valid", 600, "read", "write");
            token.Audience = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_User_not_active()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(u => u.IsActiveAsync(It.IsAny<IsActiveContext>())).Callback<IsActiveContext>(ctx=>{
                ctx.IsActive = false;
            }).Returns(Task.FromResult(0));                        

            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(tokenStore: store, users: mock.Object);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "roclient" }, "invalid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_Client_not_active()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken(new Client { ClientId = "unknown" }, "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            result.IsError.Should().BeTrue();
        }
    }
}