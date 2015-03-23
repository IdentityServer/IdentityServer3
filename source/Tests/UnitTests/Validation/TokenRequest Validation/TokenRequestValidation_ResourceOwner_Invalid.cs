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
    
    public class TokenRequestValidation_ResourceOwner_Invalid
    {
        const string Category = "TokenRequest Validation - ResourceOwner - Invalid";

        readonly IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_GrantType_For_Client()
        {
            var client = await _clients.FindClientByIdAsync("client");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UNAUTHORIZED_CLIENT);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Scopes()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.USER_NAME, "bob"},
                {Constants.TokenRequest.PASSWORD, "bob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "unknown"},
                {Constants.TokenRequest.USER_NAME, "bob"},
                {Constants.TokenRequest.PASSWORD, "bob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Scope_Multiple()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource unknown"},
                {Constants.TokenRequest.USER_NAME, "bob"},
                {Constants.TokenRequest.PASSWORD, "bob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scope()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource2"},
                {Constants.TokenRequest.USER_NAME, "bob"},
                {Constants.TokenRequest.PASSWORD, "bob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scope_Multiple()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource resource2"},
                {Constants.TokenRequest.USER_NAME, "bob"},
                {Constants.TokenRequest.PASSWORD, "bob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_ResourceOwnerCredentials()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_GRANT);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_ResourceOwner_UserName()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource"},
                {Constants.TokenRequest.PASSWORD, "bob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_GRANT);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_ResourceOwner_Password()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource"},
                {Constants.TokenRequest.USER_NAME, "bob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_GRANT);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_ResourceOwner_Credentials()
        {
            var client = await _clients.FindClientByIdAsync("roclient");
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection {
                {Constants.TokenRequest.GRANT_TYPE, Constants.GrantTypes.PASSWORD},
                {Constants.TokenRequest.SCOPE, "resource"},
                {Constants.TokenRequest.USER_NAME, "bob"},
                {Constants.TokenRequest.PASSWORD, "notbob"}
            };

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.INVALID_GRANT);
        }
    }
}