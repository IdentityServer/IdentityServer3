using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.Validation_Tests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_ResourceOwner_Invalid
    {
        const string Category = "TokenRequest Validation - ResourceOwner - Invalid";

        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();
        IUserService _users = new TestUserService();

        [TestMethod]
        [TestCategory(Category)]
        public void Invalid_GrantType_For_Client()
        {
            var client = _settings.FindClientById("client");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void No_Scopes()
        {
            var client = _settings.FindClientById("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Unknown_Scope()
        {
            var client = _settings.FindClientById("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "unknown");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Unknown_Scope_Multiple()
        {
            var client = _settings.FindClientById("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource unknown");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Restricted_Scope()
        {
            var client = _settings.FindClientById("roclient_restricted");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource2");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Restricted_Scope_Multiple()
        {
            var client = _settings.FindClientById("roclient_restricted");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource resource2");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void No_ResourceOwnerCredentials()
        {
            var client = _settings.FindClientById("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Missing_ResourceOwner_UserName()
        {
            var client = _settings.FindClientById("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Missing_ResourceOwner_Password()
        {
            var client = _settings.FindClientById("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.UserName, "bob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Invalid_ResourceOwner_Credentials()
        {
            var client = _settings.FindClientById("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "notbob");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }
    }
}