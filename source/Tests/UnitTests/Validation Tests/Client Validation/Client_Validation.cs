/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;
using UnitTests.Plumbing;

namespace UnitTests.Validation_Tests.Client_Validation
{
    [TestClass]
    public class Client_Validation
    {
        ILogger _logger = new DebugLogger();
        ICoreSettings _settings = new TestSettings();

        const string Category = "Client validation";

        [TestMethod]
        [TestCategory(Category)]
        public async Task Valid_Client_Credentials()
        {
            var credential = new ClientCredential
            {
                ClientId = "codeclient",
                Secret = "secret"
            };

            var validator = new ClientValidator(_settings, _logger);
            var client = await validator.ValidateClientAsync(credential);

            Assert.IsNotNull(client);
            Assert.AreEqual("codeclient", client.ClientId);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Invalid_Client_Credentials()
        {
            var credential = new ClientCredential
            {
                ClientId = "codeclient",
                Secret = "invalid"
            };

            var validator = new ClientValidator(_settings, _logger);
            var client = await validator.ValidateClientAsync(credential);

            Assert.IsNull(client);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [TestCategory(Category)]
        public async Task Null_Client_Credentials()
        {
            var credential = new ClientCredential();

            var validator = new ClientValidator(_settings, _logger);
            var client = await validator.ValidateClientAsync(credential);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [TestCategory(Category)]
        public async Task Null_ClientId()
        {
            var credential = new ClientCredential();
            
            var validator = new ClientValidator(_settings, _logger);
            var client = await validator.ValidateClientAsync(credential);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Empty_Client_Credentials()
        {
            var credential = new ClientCredential
            {
                ClientId = "",
                Secret = ""
            };

            var validator = new ClientValidator(_settings, _logger);
            var client = await validator.ValidateClientAsync(credential);

            Assert.IsNull(client);
        }
    }
}