/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.TokenRequest
{
    [TestClass]
    public class TokenRequestValidation_Code_Invalid
    {
        IClientStore _clients = Factory.CreateClientStore();
        const string Category = "TokenRequest Validation - AuthorizationCode - Invalid";

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Missing_AuthorizationCode()
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

            var validator = Factory.CreateTokenRequestValidator(
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
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
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
        [TestCategory(Category)]
        public async Task No_Scopes_for_AuthorizationCode()
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

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(result.Error, Constants.TokenErrors.InvalidRequest);
        }

        [TestMethod]
        [TestCategory("TokenRequest Validation - AuthorizationCode - Invalid")]
        public async Task Client_Not_Authorized_For_AuthorizationCode_Flow()
        {
            var client = await _clients.FindClientByIdAsync("implicitclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
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
            var client1 = await _clients.FindClientByIdAsync("codeclient");
            var client2 = await _clients.FindClientByIdAsync("codeclient_restricted");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client1,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
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
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
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
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server1/cb"),
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
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
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = new Uri("https://server/cb"),
                CreationTime = DateTime.UtcNow.AddSeconds(-100)
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
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
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
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

            var validator = Factory.CreateTokenRequestValidator(
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
            validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store,
                customRequestValidator: new DefaultCustomRequestValidator());
            
            result = await validator.ValidateRequestAsync(parameters, client);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual(Constants.TokenErrors.InvalidGrant, result.Error);
        }
    }
}