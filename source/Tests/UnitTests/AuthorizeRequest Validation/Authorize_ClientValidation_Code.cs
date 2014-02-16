using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests
{
    [TestClass]
    public class Authorize_ClientValidation_Code
    {
        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();

        [TestMethod]
        [TestCategory("Client Validation - Code ResponseType")]
        public void OpenId_Code_Request_Unknown_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "unknown");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.AreEqual(true, clientResult.IsError);
            Assert.AreEqual(ErrorTypes.Client, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidScope, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("Client Validation - Code ResponseType")]
        public void OpenId_Code_Request_Invalid_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://invalid");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.AreEqual(true, clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("Client Validation - Code ResponseType")]
        public void OpenId_Code_Request_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.AreEqual(true, clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("Client Validation - Code ResponseType")]
        public void OpenId_Code_Request_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.AreEqual(true, clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("Client Validation - Code ResponseType")]
        public void OpenId_Code_Request_With_Unknown_Client()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "unknown");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.AreEqual(true, clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }

        [TestMethod]
        [TestCategory("Client Validation - Code ResponseType")]
        public void OpenId_Code_Request_With_Restricted_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient_restricted");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = new AuthorizeRequestValidator(_settings, _logger);
            var protocolResult = validator.ValidateProtocol(parameters);
            Assert.AreEqual(false, protocolResult.IsError);

            var clientResult = validator.ValidateClient();
            Assert.AreEqual(true, clientResult.IsError);
            Assert.AreEqual(ErrorTypes.User, clientResult.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnauthorizedClient, clientResult.Error);
        }
    }
}