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
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Tests.Connect.Setup;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.Clients
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