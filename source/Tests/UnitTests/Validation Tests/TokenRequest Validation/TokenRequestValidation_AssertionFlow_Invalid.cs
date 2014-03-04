using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.Validation_Tests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_AssertionFlow_Invalid
    {
        const string Category = "TokenRequest Validation - AssertionFlow - Invalid";

        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();
        IAssertionGrantValidator assertionValidator = new TestAssertionValidator();

        [TestMethod]
        [TestCategory(Category)]
        public void Invalid_GrantType_For_Client()
        {
            var client = _settings.FindClientById("client");
            var validator = new TokenRequestValidator(_settings, _logger, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "assertionType");
            parameters.Add(Constants.TokenRequest.Assertion, "assertion");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Missing_Assertion()
        {
            var client = _settings.FindClientById("assertionclient");

            var validator = new TokenRequestValidator(_settings, _logger, null, null, assertionValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "assertionType");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnsupportedGrantType, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Invalid_Assertion()
        {
            var client = _settings.FindClientById("assertionclient");

            var validator = new TokenRequestValidator(_settings, _logger, null, null, assertionValidator);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "unknownAssertionType");
            parameters.Add(Constants.TokenRequest.Assertion, "unknownAssertion");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }
    }
}
