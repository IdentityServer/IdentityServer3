using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests
{
    [TestClass]
    public class AuthorizeRequest_ProtocolValidation
    {
        ILogger _logger = new DebugLogger();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_Parameter()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(null);
        }

        [TestMethod]
        public void Empty_Parameters()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var result = validator.ValidateProtocol(new NameValueCollection());

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
        }

        [TestMethod]
        public void Valid_Code_Request()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.Code;

            var result = validator.ValidateProtocol(RequestFactory.GetBaseAuthorizeRequest());
            Assert.AreEqual(false, result.IsError);
        }

        [TestMethod]
        public void Valid_Token_Request()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.Token;

            var result = validator.ValidateProtocol(RequestFactory.GetBaseAuthorizeRequest());
            Assert.AreEqual(false, result.IsError);
        }

        [TestMethod]
        public void Valid_IdToken_Request()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.IdToken;

            var result = validator.ValidateProtocol(RequestFactory.GetBaseAuthorizeRequest());
            Assert.AreEqual(false, result.IsError);
        }

        [TestMethod]
        public void Valid_IdToken_With_FormPost_ResponseMode_Request()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.IdToken;
            request[Constants.AuthorizeRequest.ResponseMode] = Constants.ResponseModes.FormPost;

            var result = validator.ValidateProtocol(RequestFactory.GetBaseAuthorizeRequest());
            Assert.AreEqual(false, result.IsError);
        }

        [TestMethod]
        public void Valid_IdTokenToken_Request()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.IdTokenToken;

            var result = validator.ValidateProtocol(RequestFactory.GetBaseAuthorizeRequest());
            Assert.AreEqual(false, result.IsError);
        }

        [TestMethod]
        public void Missing_ClientId()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.ClientId);
            var result = validator.ValidateProtocol(request);
            
            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);

        }

        [TestMethod]
        public void Missing_Scope()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.Scope);
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        public void Missing_RedirectUri()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.RedirectUri);
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
        }

        [TestMethod]
        public void Invalid_RedirectUri()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.RedirectUri] = "invalid";
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
        }

        [TestMethod]
        public void Missing_ResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request.Remove(Constants.AuthorizeRequest.ResponseType);
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        public void Invalid_ResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = "invalid";
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        public void Invalid_ResponseMode_For_Code_ResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.Code;
            request[Constants.AuthorizeRequest.ResponseMode] = Constants.ResponseModes.FormPost;
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        public void Invalid_ResponseMode_For_Code_TokenResponseType()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.ResponseType] = Constants.ResponseTypes.Token;
            request[Constants.AuthorizeRequest.ResponseMode] = Constants.ResponseModes.FormPost;
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }

        [TestMethod]
        public void Invalid_MaxAge()
        {
            var validator = new AuthorizeRequestValidator(null, _logger);
            var request = RequestFactory.GetBaseAuthorizeRequest();

            request[Constants.AuthorizeRequest.MaxAge] = "invalid";
            var result = validator.ValidateProtocol(request);

            Assert.AreEqual(true, result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
        }
    }
}