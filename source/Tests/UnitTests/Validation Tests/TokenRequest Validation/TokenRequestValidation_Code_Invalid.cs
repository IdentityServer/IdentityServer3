using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
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
        public void Missing_AuthorizationCode()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "codeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken()
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public void Invalid_AuthorizationCode()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "codeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken()
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "invalid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public void Client_Not_Authorized_For_AuthorizationCode_Flow()
        {
            var client = _settings.FindClientById("implicitclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "codeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken()
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public void Client_Trying_To_Request_Token_Using_Another_Clients_Code()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "othercodeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken()
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public void Missing_RedirectUri()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "codeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken()
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public void Different_RedirectUri_Between_Authorize_And_Token_Request()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "codeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server1/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken()
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server2/cb");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.UnauthorizedClient, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public void Expired_AuthorizationCode()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "codeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken(),

                CreationTime = DateTime.UtcNow.AddSeconds(-100)
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, null);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public void Reused_AuthorizationCode()
        {
            var client = _settings.FindClientById("codeclient");
            var store = new TestCodeStore();

            var code = new AuthorizationCode
            {
                ClientId = "codeclient",
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),

                AccessToken = TokenFactory.CreateAccessToken(),
                IdentityToken = TokenFactory.CreateIdentityToken(),
            };

            store.Store("valid", code);

            var validator = new TokenRequestValidator(_settings, _logger, store, null, null, new DefaultCustomRequestValidator());

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            // request first time
            var result = validator.ValidateRequest(parameters, client);

            Assert.IsFalse(result.IsError);

            // request second time
            validator = new TokenRequestValidator(_settings, _logger, store, null, null, new DefaultCustomRequestValidator());
            result = validator.ValidateRequest(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }
    }
}