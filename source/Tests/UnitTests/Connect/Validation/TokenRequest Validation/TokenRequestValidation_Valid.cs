/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using UnitTests.Plumbing;

namespace UnitTests.TokenRequest_Validation
{
    [TestClass]
    public class TokenRequestValidation_Valid
    {
        const string Category = "TokenRequest Validation - General - Valid";

        IClientStore _clients = Factory.CreateClientStore();

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_Code_Request()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                RedirectUri = new Uri("https://server/cb"),
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_Code_Request_with_Refresh_Token()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                RedirectUri = new Uri("https://server/cb"),
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    },
                    new Scope
                    {
                        Name = "offline_access"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_ClientCredentials_Request()
        {
            var client = await _clients.FindClientByIdAsync("client");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_ClientCredentials_Request_Restricted_Client()
        {
            var client = await _clients.FindClientByIdAsync("client_restricted");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.ClientCredentials);
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_ResourceOwner_Request()
        {
            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_ResourceOwner_Request_with_Refresh_Token()
        {
            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");
            parameters.Add(Constants.TokenRequest.Scope, "resource offline_access");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_ResourceOwner_Request_Restricted_Client()
        {
            var client = await _clients.FindClientByIdAsync("roclient_restricted");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.Password);
            parameters.Add(Constants.TokenRequest.UserName, "bob");
            parameters.Add(Constants.TokenRequest.Password, "bob");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_AssertionFlow_Request()
        {
            var client = await _clients.FindClientByIdAsync("assertionclient");

            var validator = Factory.CreateTokenValidator();

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "assertionType");
            parameters.Add(Constants.TokenRequest.Assertion, "assertion");
            parameters.Add(Constants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_RefreshToken_Request()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token"),
                ClientId = "roclient",
                LifeTime = 600,
                Handle = Guid.NewGuid().ToString(),
                CreationTime = DateTime.UtcNow
            };

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(refreshToken.Handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient");

            var validator = Factory.CreateTokenValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, refreshToken.Handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_RefreshToken_Request_using_Restricted_Client()
        {
            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token"),
                ClientId = "roclient_restricted_refresh",
                LifeTime = 600,
                Handle = Guid.NewGuid().ToString(),
                CreationTime = DateTime.UtcNow
            };

            var store = new InMemoryRefreshTokenStore();
            await store.StoreAsync(refreshToken.Handle, refreshToken);

            var client = await _clients.FindClientByIdAsync("roclient_restricted_refresh");

            var validator = Factory.CreateTokenValidator(
                refreshTokens: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(Constants.TokenRequest.RefreshToken, refreshToken.Handle);

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsFalse(result.IsError);
        }
    }
}