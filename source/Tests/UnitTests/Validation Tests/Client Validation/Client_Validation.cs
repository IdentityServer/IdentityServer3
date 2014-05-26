/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Models;
using UnitTests.Plumbing;

namespace UnitTests.Validation_Tests.Client_Validation
{
    [TestClass]
    public class Client_Validation
    {
        ClientValidator _validator = Factory.CreateClientValidator();

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

            var client = await _validator.ValidateClientCredentialsAsync(credential);

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

            var client = await _validator.ValidateClientCredentialsAsync(credential);

            Assert.IsNull(client);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Unkown_Client()
        {
            var credential = new ClientCredential
            {
                ClientId = "unknown",
                Secret = "invalid"
            };

            var client = await _validator.ValidateClientCredentialsAsync(credential);

            Assert.IsNull(client);
        }

        [TestMethod]
        [TestCategory(Category)]
        public async Task Disabled_Client()
        {
            var credential = new ClientCredential
            {
                ClientId = "disabled",
                Secret = "invalid"
            };

            var client = await _validator.ValidateClientCredentialsAsync(credential);

            Assert.IsNull(client);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [TestCategory(Category)]
        public async Task Null_Client_Credentials()
        {
            var credential = new ClientCredential();

            var client = await _validator.ValidateClientCredentialsAsync(credential);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [TestCategory(Category)]
        public async Task Null_ClientId()
        {
            var credential = new ClientCredential();
            
            var client = await _validator.ValidateClientCredentialsAsync(credential);
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

            var client = await _validator.ValidateClientCredentialsAsync(credential);

            Assert.IsNull(client);
        }
    }
}