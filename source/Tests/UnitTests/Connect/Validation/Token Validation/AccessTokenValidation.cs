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

using Moq;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Tests.Connect.Setup;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.Tokens
{
    public class AccessTokenValidation
    {
        const string Category = "Access token validation";

        IClientStore _clients = Factory.CreateClientStore();

        static AccessTokenValidation()
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken("roclient", "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            Assert.False(result.IsError);
            Assert.Equal(8, result.Claims.Count());
            Assert.Equal("roclient", result.Claims.First(c => c.Type == Constants.ClaimTypes.ClientId).Value);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_required_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken("roclient", "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "read");

            Assert.False(result.IsError);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Reference_Token_with_missing_Scope()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken("roclient", "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123", "missing");

            Assert.True(result.IsError);
            Assert.Equal(Constants.ProtectedResourceErrors.InsufficientScope, result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unknown");

            Assert.True(result.IsError);
            Assert.Equal(Constants.ProtectedResourceErrors.InvalidToken, result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_Reference_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken("roclient", "valid", 2, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);
            await Task.Delay(2000);

            var result = await validator.ValidateAccessTokenAsync("123");

            Assert.True(result.IsError);
            Assert.Equal(Constants.ProtectedResourceErrors.ExpiredToken, result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Malformed_JWT_Token()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var result = await validator.ValidateAccessTokenAsync("unk.nown");

            Assert.True(result.IsError);
            Assert.Equal(Constants.ProtectedResourceErrors.InvalidToken, result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_JWT_Token()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateAccessToken("roclient", "valid", 600, "read", "write"));

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            Assert.False(result.IsError);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Issuer()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var token = TokenFactory.CreateAccessToken("roclient", "valid", 600, "read", "write");
            token.Issuer = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            Assert.True(result.IsError);
            Assert.Equal(Constants.ProtectedResourceErrors.InvalidToken, result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task JWT_Token_invalid_Audience()
        {
            var signer = new DefaultTokenSigningService(TestIdentityServerOptions.Create());
            var token = TokenFactory.CreateAccessToken("roclient", "valid", 600, "read", "write");
            token.Audience = "invalid";
            var jwt = await signer.SignTokenAsync(token);

            var validator = Factory.CreateTokenValidator(null);
            var result = await validator.ValidateAccessTokenAsync(jwt);

            Assert.True(result.IsError);
            Assert.Equal(Constants.ProtectedResourceErrors.InvalidToken, result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_User_not_active()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(u => u.IsActiveAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(false));                        

            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(tokenStore: store, users: mock.Object);

            var token = TokenFactory.CreateAccessToken("roclient", "invalid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            Assert.True(result.IsError);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_AccessToken_but_Client_not_active()
        {
            var store = new InMemoryTokenHandleStore();
            var validator = Factory.CreateTokenValidator(store);

            var token = TokenFactory.CreateAccessToken("unknown", "valid", 600, "read", "write");
            var handle = "123";

            await store.StoreAsync(handle, token);

            var result = await validator.ValidateAccessTokenAsync("123");

            Assert.True(result.IsError);
        }
    }
}