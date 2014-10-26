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
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.AuthorizeRequest
{
    [TestClass]
    public class Authorize_ClientValidation_Valid
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_OpenId_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Resource_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_Code_Request_Multiple_Scopes()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_OpenId_IdTokenToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_IdTokenToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_IdTokenToken_Request_Multiple_Scopes()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Resource_Token_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = Factory.CreateAuthorizeRequestValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }
    }
}