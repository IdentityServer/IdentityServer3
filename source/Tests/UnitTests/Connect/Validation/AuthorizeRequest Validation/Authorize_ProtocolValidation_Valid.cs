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
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.AuthorizeRequest
{
    [TestClass]
    public class Authorize_ProtocolValidation_Valid
    {
        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_OpenId_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.AreEqual(false, result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_Resource_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_Mixed_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_Resource_Token_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_OpenId_IdToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        // is valid because protocol validation on its own cannot know about resource scopes
        public void Valid_Mixed_IdToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_OpenId_IdTokenToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, Constants.StandardScopes.OpenId);
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_Mixed_IdTokenToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_OpenId_IdToken_With_FormPost_ResponseMode_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, Constants.StandardScopes.OpenId);
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.FormPost);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation - Valid")]
        public void Valid_OpenId_IdToken_Token_With_FormPost_ResponseMode_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.FormPost);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsFalse(result.IsError);
        }
    }
}