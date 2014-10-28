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
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    [TestClass]
    public class DefaultConsentServiceTests
    {
        DefaultConsentService subject;
        InMemoryConsentStore store;
        ClaimsPrincipal user;
        Client client;
        List<string> scopes;

        [TestInitialize]
        public void Init()
        {
            scopes = new List<string> { "read", "write" };
            client = new Client {ClientId = "client", AllowRememberConsent = true, RequireConsent = true};
            user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]{new Claim(Constants.ClaimTypes.Subject, "123")}, "password"));
            store = new InMemoryConsentStore();
            subject = new DefaultConsentService(store);
        }

        [TestMethod]
        public void RequiresConsentAsync_NoPriorConsentGiven_ReturnsTrue()
        {
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RequiresConsentAsync_PriorConsentGiven_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RequiresConsentAsync_PriorConsentGiven_ScopesInDifferentOrder_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.Reverse();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RequiresConsentAsync_PriorConsentGiven_MoreScopesRequested_ReturnsTrue()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.Add("query");
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RequiresConsentAsync_PriorConsentGiven_FewerScopesRequested_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.RemoveAt(0);
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public void RequiresConsentAsync_PriorConsentGiven_NoScopesRequested_ReturnsFalse()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            scopes.Clear();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RequiresConsentAsync_ClientDoesNotRequireConsent_ReturnsFalse()
        {
            client.RequireConsent = false;
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RequiresConsentAsync_ClientDoesNotAllowRememberConsent_ReturnsTrue()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            client.AllowRememberConsent = false;
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateConsentAsync_ConsentGiven_ConsentNoLongerRequired()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var result =  subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdateConsentAsync_ClientDoesNotAllowRememberConsent_ConsentStillRequired()
        {
            client.AllowRememberConsent = false;
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateConsentAsync_PriorConsentGiven_NullScopes_ConsentNowRequired()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            subject.UpdateConsentAsync(client, user, null).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateConsentAsync_PriorConsentGiven_EmptyScopeCollection_ConsentNowRequired()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            subject.UpdateConsentAsync(client, user, new string[0]).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void UpdateConsentAsync_ChangeConsent_OldConsentNotAllowed()
        {
            subject.UpdateConsentAsync(client, user, scopes).Wait();
            var newConsent = new string[] { "foo", "bar" };
            subject.UpdateConsentAsync(client, user, newConsent).Wait();
            var result = subject.RequiresConsentAsync(client, user, scopes).Result;
            Assert.IsTrue(result);
        }
    }
}
