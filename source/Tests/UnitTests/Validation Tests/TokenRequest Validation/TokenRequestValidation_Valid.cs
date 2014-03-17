using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_Valid
    {
        const string Category = "TokenRequest Validation - General - Valid";

        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();
        IUserService _users = new TestUserService();
        ICustomRequestValidator _customRequestValidator = new DefaultCustomRequestValidator();

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_Code_Request()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            store.Store("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store,
                customRequestValidator: _customRequestValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_ClientCredentials_Request()
        {
            var client = _settings.FindClientById("client");

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                customRequestValidator: _customRequestValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_ClientCredentials_Request_Restricted_Client()
        {
            var client = _settings.FindClientById("client_restricted");

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                customRequestValidator: _customRequestValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_ResourceOwner_Request()
        {
            var client = _settings.FindClientById("roclient");

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users,
                customRequestValidator: _customRequestValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_ResourceOwner_Request_Restricted_Client()
        {
            var client = _settings.FindClientById("roclient_restricted");

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users,
                customRequestValidator: _customRequestValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_AssertionFlow_Request()
        {
            var client = _settings.FindClientById("assertionclient");

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                assertionGrantValidator: new TestAssertionValidator(),
                customRequestValidator: _customRequestValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "assertionType");
            parameters.Add(Constants.TokenRequest.Assertion, "assertion");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsFalse(result.IsError);
        }
    }
}