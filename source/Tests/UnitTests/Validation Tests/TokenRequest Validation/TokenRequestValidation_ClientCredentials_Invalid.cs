using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.Validation_Tests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_ClientCredentials_Invalid
    {
        const string Category = "TokenRequest Validation - ClientCredentials - Invalid";

        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();
        IUserService _users = new TestUserService();

        [TestMethod]
        [TestCategory(Category)]
        public void Invalid_GrantType_For_Client()
        {
            var client = _settings.FindClientById("client");
            var validator = new TokenRequestValidator(_settings, _logger, null, null);

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
            var client = _settings.FindClientById("client");
            var validator = new TokenRequestValidator(_settings, _logger, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Unknown_Scope()
        {
            var client = _settings.FindClientById("client");
            var validator = new TokenRequestValidator(_settings, _logger, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "unknown");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Unknown_Scope_Multiple()
        {
            var client = _settings.FindClientById("client");
            var validator = new TokenRequestValidator(_settings, _logger, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "resource unknown");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Restricted_Scope()
        {
            var client = _settings.FindClientById("client_restricted");
            var validator = new TokenRequestValidator(_settings, _logger, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "resource2");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Restricted_Scope_Multiple()
        {
            var client = _settings.FindClientById("client_restricted");
            var validator = new TokenRequestValidator(_settings, _logger, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "resource resource2");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Identity_Scope()
        {
            var client = _settings.FindClientById("client");
            var validator = new TokenRequestValidator(_settings, _logger, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "openid");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }
    }
}
