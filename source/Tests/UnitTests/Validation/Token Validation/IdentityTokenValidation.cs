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
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.Tokens
{

    public class IdentityTokenValidation
    {
        const string Category = "Identity token validation";

        static IdentityTokenValidation()
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_IdentityToken_DefaultKeyType()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_IdentityToken_DefaultKeyType_no_ClientId_supplied()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_IdentityToken_no_ClientId_supplied()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt);
            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task IdentityToken_InvalidClientId()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "invalid");
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task IdentityToken_Too_Long()
        {
            var signer = Factory.CreateDefaultTokenSigningService();
            var jwt = await signer.SignTokenAsync(TokenFactory.CreateIdentityTokenLong("roclient", "valid", 1000));
            var validator = Factory.CreateTokenValidator();

            var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }
    }
}