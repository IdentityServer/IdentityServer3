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
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.AuthorizeRequest
{
    [TestClass]
    public class Authorize_ClientValidation_Code
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Code")]
        public async Task Code_Request_Unknown_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "unknown");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.Client, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidScope, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://invalid");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Unknown_Client()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "unknown");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Restricted_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient_restricted");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }
    }
}