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
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.AuthorizeRequest
{
    public class Authorize_ProtocolValidation_Invalid
    {
        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        
        public void Null_Parameter()
        {
            var validator = Factory.CreateAuthorizeRequestValidator();

            Action act = () => validator.ValidateProtocol(null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Empty_Parameters()
        {
            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(new NameValueCollection());

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        // fails because openid scope is requested, but no response type that indicates an identity token
        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void OpenId_Token_Only_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, Constants.StandardScopes.OpenId);
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Resource_Only_IdToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Mixed_Token_Only_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void OpenId_IdToken_Request_Nonce_Missing()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Missing_ClientId()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Missing_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Missing_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Malformed_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "malformed");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Malformed_RedirectUri_Triple_Slash()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https:///attacker.com");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }


        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Missing_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Unknown_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, "unknown");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_Code_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.Fragment);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.Query);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.Query);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_CodeToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.CodeToken);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.Query);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_CodeIdToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.CodeIdToken);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.Query);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_CodeIdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.CodeIdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.Query);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnsupportedResponseType);
        }


        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Malformed_MaxAge()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);
            parameters.Add(Constants.AuthorizeRequest.MaxAge, "malformed");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Negative_MaxAge()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);
            parameters.Add(Constants.AuthorizeRequest.MaxAge, "-1");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }
    }
}