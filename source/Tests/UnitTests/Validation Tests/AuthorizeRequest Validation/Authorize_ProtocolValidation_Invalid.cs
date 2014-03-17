using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests
{
    [TestClass]
    public class Authorize_ProtocolValidation_Invalid
    {
        ILogger _logger = new DebugLogger();

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_Parameter()
        {
            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(null);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Empty_Parameters()
        {
            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(new NameValueCollection());

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        // fails because openid scope is requested, but no response type that indicates an identity token
        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void OpenId_Token_Only_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, Constants.StandardScopes.OpenId);
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Resource_Only_IdToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Mixed_Token_Only_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void OpenId_IdToken_Request_Nonce_Missing()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Missing_ClientId()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Missing_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Missing_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Malformed_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "malformed");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Malformed_RedirectUri_Triple_Slash()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https:///attacker.com");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.User, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }


        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Missing_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnsupportedResponseType, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Unknown_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, "unknown");

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnsupportedResponseType, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_Code_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.FormPost);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.UnsupportedResponseType, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Invalid_ResponseMode_For_Token_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);
            parameters.Add(Constants.AuthorizeRequest.ResponseMode, Constants.ResponseModes.FormPost);

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Malformed_MaxAge()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);
            parameters.Add(Constants.AuthorizeRequest.MaxAge, "malformed");

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }

        [TestMethod]
        [TestCategory("AuthorizeRequest Protocol Validation")]
        public void Negative_MaxAge()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);
            parameters.Add(Constants.AuthorizeRequest.MaxAge, "-1");

            var validator = ValidatorFactory.CreateAuthorizeValidator();
            var result = validator.ValidateProtocol(parameters);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InvalidRequest, result.Error);
        }
    }
}