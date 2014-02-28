using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

        [TestMethod]
        public void Valid_Client_Credentials()
        {
            var credential = new ClientCredential
            {
                ClientId = "codeclient",
                Secret = "secret"
            };

            var validator = new ClientValidator(_settings, _logger);
            var client = validator.ValidateClient(credential);

            Assert.IsNotNull(client);
            Assert.AreEqual("codeclient", client.ClientId);
        }

        [TestMethod]
        public void Invalid_Client_Credentials()
        {
            var credential = new ClientCredential
            {
                ClientId = "codeclient",
                Secret = "invalid"
            };

            var validator = new ClientValidator(_settings, _logger);
            var client = validator.ValidateClient(credential);

            Assert.IsNull(client);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Null_Client_Credentials()
        {
            var credential = new ClientCredential();

            var validator = new ClientValidator(_settings, _logger);
            var client = validator.ValidateClient(credential);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Null_ClientId()
        {
            var credential = new ClientCredential();
            
            var validator = new ClientValidator(_settings, _logger);
            var client = validator.ValidateClient(credential);
        }

        [TestMethod]
        public void Empty_Client_Credentials()
        {
            var credential = new ClientCredential
            {
                ClientId = "",
                Secret = ""
            };

            var validator = new ClientValidator(_settings, _logger);
            var client = validator.ValidateClient(credential);

            Assert.IsNull(client);
        }
    }
}
