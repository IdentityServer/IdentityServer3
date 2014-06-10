/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.Validation_Tests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_AssertionFlow_Invalid
    {
        const string Category = "TokenRequest Validation - AssertionFlow - Invalid";

        IClientService _clients = Factory.CreateClientService();

        [TestMethod]
        [TestCategory(Category)]
        public async Task Invalid_GrantType_For_Client()
        {
            var client = await _clients.FindClientByIdAsync("client");
            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "assertionType");
            parameters.Add(Constants.TokenRequest.Assertion, "assertion");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Missing_Assertion()
        {
            var client = await _clients.FindClientByIdAsync("assertionclient");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "assertionType");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnsupportedGrantType, result.Error);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Invalid_Assertion()
        {
            var client = await _clients.FindClientByIdAsync("assertionclient");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "unknownAssertionType");
            parameters.Add(Constants.TokenRequest.Assertion, "unknownAssertion");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }
    }
}