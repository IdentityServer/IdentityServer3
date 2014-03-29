using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
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
        public async Task Invalid_GrantType_For_Client()
        {
            var client = await _settings.FindClientByIdAsync("client");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task No_Scopes()
        {
            var client = await _settings.FindClientByIdAsync("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Unknown_Scope()
        {
            var client = await _settings.FindClientByIdAsync("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "unknown");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Unknown_Scope_Multiple()
        {
            var client = await _settings.FindClientByIdAsync("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource unknown");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Restricted_Scope()
        {
            var client = await _settings.FindClientByIdAsync("roclient_restricted");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource2");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Restricted_Scope_Multiple()
        {
            var client = await _settings.FindClientByIdAsync("roclient_restricted");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource resource2");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidScope, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task No_ResourceOwnerCredentials()
        {
            var client = await _settings.FindClientByIdAsync("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Missing_ResourceOwner_UserName()
        {
            var client = await _settings.FindClientByIdAsync("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.Password, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Missing_ResourceOwner_Password()
        {
            var client = await _settings.FindClientByIdAsync("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.UserName, "bob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Invalid_ResourceOwner_Credentials()
        {
            var client = await _settings.FindClientByIdAsync("roclient");
            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                userService: _users);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.Scope, "resource");
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "notbob");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }
    }
}