/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect;
using UnitTests.Plumbing;

namespace UnitTests
{
    [TestClass]
    public class Authorize_ClientValidation_Code
    {
        CoreSettings _settings = new TestSettings();

        [TestMethod]
        [TestCategory("AuthorizeRequest Client Validation - Code")]
        public async Task Code_Request_Unknown_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "unknown");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
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

            var validator = Factory.CreateAuthorizeValidator();
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.IsFalse(protocolResult.IsError);

            var clientResult = await validator.ValidateClientAsync();
            Assert.IsTrue(clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }
    }
}