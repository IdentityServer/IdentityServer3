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

using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Tests.Validation.Setup;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.TokenRequest_Validation
{
    
    public class TokenRequestValidation_CustomGrants_Invalid
    {
        const string Category = "TokenRequest Validation - AssertionFlow - Invalid";

        readonly IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Custom_Grant_Type_For_Client_Credentials_Client()
        {
            var client = await _clients.FindClientByIdAsync("client");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, "customGrant"},
                {Constants.TokenRequest.SCOPE, "resource"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UNSUPPORTED_GRANT_TYPE);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Custom_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, "unknown_grant_type"},
                {Constants.TokenRequest.SCOPE, "resource"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UNSUPPORTED_GRANT_TYPE);
        }
    }
}