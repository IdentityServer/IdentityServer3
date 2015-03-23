﻿/*
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
    
    public class Authorize_ClientValidation_IdToken
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - IdToken")]
        public async Task Mixed_IdToken_Request()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "implicitclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid resource"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "oob://implicit/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.ID_TOKEN},
                {Constants.AuthorizeRequest.NONCE, "abc"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);
            
            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_SCOPE);
        }
    }
}