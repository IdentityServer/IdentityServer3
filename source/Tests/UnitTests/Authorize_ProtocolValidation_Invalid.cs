using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests
{
    [TestClass]
    public class Authorize_ProtocolValidation_Invalid
    {
        ILogger _logger = new DebugLogger();

        [TestMethod]
        [TestCategory("Protocol Validation")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_Parameter()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(null);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Empty_Parameters()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(new NameValueCollection());

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
        }

        // fails because openid scope is requested, but no response type that indicates an identity token
        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void OpenId_Token_Only_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, Constants.StandardScopes.OpenId);
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(parameters);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Resource_Only_IdToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(parameters);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Mixed_Token_Only_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(parameters);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void OpenId_IdToken_Request_Nonce_Missing()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);

            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(parameters);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Missing_ClientId()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.ClientId);
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);

        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Missing_Scope()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.Scope);
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Missing_RedirectUri()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.RedirectUri);
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Malformed_RedirectUri()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request[Constants.AuthorizeRequest.RedirectUri] = "invalid";
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Missing_ResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.ResponseType);
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Malformed_ResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = "invalid";
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Invalid_ResponseMode_For_Code_ResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.Code;
            request[Constants.AuthorizeRequest.ResponseMode] = Constants.ResponseModes.FormPost;
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Invalid_ResponseMode_For_Token_ResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.Token;
            request[Constants.AuthorizeRequest.ResponseMode] = Constants.ResponseModes.FormPost;
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        [TestCategory("Protocol Validation")]
        public void Malformed_MaxAge()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetMinimalAuthorizeRequest();

            request[Constants.AuthorizeRequest.MaxAge] = "invalid";
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }
    }
}