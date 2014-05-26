/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Tests.Plumbing;
using UnitTests.Plumbing;

namespace UnitTests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_General_Invalid
    {
        IClientService _clients = new InMemoryClientService(TestClients.Get());

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("TokenRequest Validation - General - Invalid")]
        public async Task Parameters_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenValidator(
                authorizationCodeStore: store);

            var result = await validator.ValidateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("TokenRequest Validation - General - Invalid")]
        public async Task Client_Null()
        {
            var store = new InMemoryAuthorizationCodeStore();
            var validator = Factory.CreateTokenValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, null);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - General - Invalid")]
        public async Task Unknown_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "unknown");
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnsupportedGrantType, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - General - Invalid")]
        public async Task Missing_Grant_Type()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnsupportedGrantType, result.Error);
        }
    }
}