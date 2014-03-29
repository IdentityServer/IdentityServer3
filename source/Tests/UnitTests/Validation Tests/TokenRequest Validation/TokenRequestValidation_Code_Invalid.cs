using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_Code_Invalid
    {
        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Missing_AuthorizationCode()
        {
            var client = await _settings.FindClientByIdAsync("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Invalid_AuthorizationCode()
        {
            var client = await _settings.FindClientByIdAsync("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "invalid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Client_Not_Authorized_For_AuthorizationCode_Flow()
        {
            var client = await _settings.FindClientByIdAsync("implicitclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Client_Trying_To_Request_Token_Using_Another_Clients_Code()
        {
            var client1 = await _settings.FindClientByIdAsync("codeclient");
            var client2 = await _settings.FindClientByIdAsync("codeclient_restricted");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client1,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client2);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Missing_RedirectUri()
        {
            var client = await _settings.FindClientByIdAsync("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Different_RedirectUri_Between_Authorize_And_Token_Request()
        {
            var client = await _settings.FindClientByIdAsync("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server1/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server2/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Expired_AuthorizationCode()
        {
            var client = await _settings.FindClientByIdAsync("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
                CreationTime = DateTime.UtcNow.AddSeconds(-100)
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Reused_AuthorizationCode()
        {
            var client = await _settings.FindClientByIdAsync("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store,
                customRequestValidator: new DefaultCustomRequestValidator());

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            // request first time
            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);

            // request second time
            validator = ValidatorFactory.CreateTokenValidator(_settings, _logger,
                authorizationCodeStore: store,
                customRequestValidator: new DefaultCustomRequestValidator());
            
            result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }
    }
}