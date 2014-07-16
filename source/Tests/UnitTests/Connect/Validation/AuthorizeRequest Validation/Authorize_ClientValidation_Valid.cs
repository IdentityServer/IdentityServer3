/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using UnitTests.Plumbing;

namespace UnitTests
{
    [TestClass]
    public class Authorize_ClientValidation_Valid
    {
        CoreSettings _settings = new TestSettings();

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_OpenId_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsFalse(clientResult.IsError);
        }
    }
}