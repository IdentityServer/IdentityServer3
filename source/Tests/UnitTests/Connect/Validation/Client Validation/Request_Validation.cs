/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using Thinktecture.IdentityModel.Http;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect;
using UnitTests.Plumbing;

namespace UnitTests.Validation_Tests.Client_Validation
{
    [TestClass]
    public class Request_Validation
    {
        const string Category = "Client Credentials - Request Validation";

        ClientValidator _validator = Factory.CreateClientValidator();

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_BasicAuthentication_Request()
        {
            var header = new BasicAuthenticationHeaderValue("client", "secret");

            var credential = _validator.ValidateHttpRequest(header, null);

            Assert.IsFalse(credential.IsMalformed);
            Assert.IsTrue(credential.IsPresent);
            Assert.AreEqual(Constants.ClientAuthenticationMethods.Basic, credential.Type);

            Assert.AreEqual("client", credential.ClientId);
            Assert.AreEqual("secret", credential.Secret);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_FormPost_Request()
        {
            var body = new NameValueCollection();
            body.Add("client_id", "client");
            body.Add("client_secret", "secret");

            var credential = _validator.ValidateHttpRequest(null, body);

            Assert.IsFalse(credential.IsMalformed);
            Assert.IsTrue(credential.IsPresent);
            Assert.AreEqual(Constants.ClientAuthenticationMethods.FormPost, credential.Type);

            Assert.AreEqual("client", credential.ClientId);
            Assert.AreEqual("secret", credential.Secret);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Valid_BasicAuthentication_and_FormPost_Request()
        {
            var header = new BasicAuthenticationHeaderValue("client", "secret");

            var body = new NameValueCollection();
            body.Add("client_id", "client");
            body.Add("client_secret", "secret");

            var credential = _validator.ValidateHttpRequest(header, null);

            Assert.IsFalse(credential.IsMalformed);
            Assert.IsTrue(credential.IsPresent);
            Assert.AreEqual(Constants.ClientAuthenticationMethods.Basic, credential.Type);

            Assert.AreEqual("client", credential.ClientId);
            Assert.AreEqual("secret", credential.Secret);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void No_Client_Credentials()
        {
            var credential = _validator.ValidateHttpRequest(null, null);

            Assert.IsFalse(credential.IsMalformed);
            Assert.IsFalse(credential.IsPresent);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void BasicAuthentication_Request_With_Empty_Basic_Header()
        {
            var header = new AuthenticationHeaderValue("Basic");

            var credential = _validator.ValidateHttpRequest(header, null);

            Assert.IsTrue(credential.IsMalformed);
            Assert.IsFalse(credential.IsPresent);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void BasicAuthentication_Request_With_Unknown_Scheme()
        {
            var header = new AuthenticationHeaderValue("Unkown", "data");

            var credential = _validator.ValidateHttpRequest(header, null);

            Assert.IsFalse(credential.IsMalformed);
            Assert.IsFalse(credential.IsPresent);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void BasicAuthentication_Request_With_Malformed_Credentials_NoBase64_Encoding()
        {
            var header = new AuthenticationHeaderValue("Basic", "somerandomdata");

            var credential = _validator.ValidateHttpRequest(header, null);

            Assert.IsTrue(credential.IsMalformed);
            Assert.IsFalse(credential.IsPresent);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only()
        {
            var invalidCred = "username";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidCred));
            var header = new AuthenticationHeaderValue("Basic", encoded);

            var credential = _validator.ValidateHttpRequest(header, null);

            Assert.IsTrue(credential.IsMalformed);
            Assert.IsFalse(credential.IsPresent);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only_With_Colon()
        {
            var invalidCred = "username:";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidCred));
            var header = new AuthenticationHeaderValue("Basic", encoded);

            var credential = _validator.ValidateHttpRequest(header, null);

            Assert.IsTrue(credential.IsMalformed);
            Assert.IsFalse(credential.IsPresent);
        }
    }
}