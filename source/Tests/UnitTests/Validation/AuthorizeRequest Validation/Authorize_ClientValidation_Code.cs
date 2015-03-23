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
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Tests.Validation.Setup;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.AuthorizeRequest_Validation
{
    
    public class Authorize_ClientValidation_Code
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task Code_Request_Unknown_Scope()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "unknown"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_RedirectUri()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://invalid"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.ID_TOKEN},
                {Constants.AuthorizeRequest.NONCE, "abc"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.ID_TOKEN_TOKEN},
                {Constants.AuthorizeRequest.NONCE, "abc"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Unknown_Client()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "unknown"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Restricted_Scope()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient_restricted"},
                {Constants.AuthorizeRequest.SCOPE, "openid profile"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
        }
    }
}