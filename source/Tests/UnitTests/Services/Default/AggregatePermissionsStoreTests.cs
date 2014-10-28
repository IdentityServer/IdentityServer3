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
using System.Linq;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    [TestClass]
    public class AggregatePermissionsStoreTests
    {
        AggregatePermissionsStore subject;
        InMemoryConsentStore store1;
        InMemoryConsentStore store2;

        [TestInitialize]
        public void Init()
        {
            store1 = new InMemoryConsentStore();
            store2 = new InMemoryConsentStore();
            subject = new AggregatePermissionsStore(store1, store2);
        }

        [TestMethod]
        public void LoadAllAsync_EmptyStores_ReturnsEmptyConsentCollection()
        {
            var result = subject.LoadAllAsync("sub").Result;
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void LoadAllAsync_OnlyOneStoreHasConsent_ReturnsSameConsent()
        {
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new string[] { "foo", "bar" } });

            var result = subject.LoadAllAsync("sub").Result;
            Assert.AreEqual(1, result.Count());
            var consent = result.First();
            Assert.AreEqual("sub", consent.Subject);
            Assert.AreEqual("client", consent.ClientId);
            CollectionAssert.AreEquivalent(new string[] { "foo", "bar" }, consent.Scopes.ToArray());
        }
        
        [TestMethod]
        public void LoadAllAsync_StoresHaveSameConsent_ReturnsSameConsent()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new string[] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new string[] { "foo", "bar" } });

            var result = subject.LoadAllAsync("sub").Result;
            Assert.AreEqual(1, result.Count());
            var consent = result.First();
            Assert.AreEqual("sub", consent.Subject);
            Assert.AreEqual("client", consent.ClientId);
            CollectionAssert.AreEquivalent(new string[] { "foo", "bar" }, consent.Scopes.ToArray());
        }
        
        [TestMethod]
        public void LoadAllAsync_StoresHaveOverlappingConsent_ReturnsCorrectUnion()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new string[] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new string[] { "bar", "baz" } });

            var result = subject.LoadAllAsync("sub").Result;
            Assert.AreEqual(1, result.Count());
            var consent = result.First();
            Assert.AreEqual("sub", consent.Subject);
            Assert.AreEqual("client", consent.ClientId);
            CollectionAssert.AreEquivalent(new string[] { "foo", "bar", "baz" }, consent.Scopes.ToArray());
        }

        [TestMethod]
        public void LoadAllAsync_BothStoresHaveDifferentConsent_ReturnsCorrectUnion()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new string[] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new string[] { "quux", "baz" } });

            var result = subject.LoadAllAsync("sub").Result;
            Assert.AreEqual(1, result.Count());
            var consent = result.First();
            Assert.AreEqual("sub", consent.Subject);
            Assert.AreEqual("client", consent.ClientId);
            CollectionAssert.AreEquivalent(new string[] { "foo", "bar", "baz", "quux" }, consent.Scopes.ToArray());
        }
        
        [TestMethod]
        public void LoadAllAsync_StoresHaveMultipleClientConsent_ReturnsCorrectConsent()
        {
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "foo1" } });
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new string[] { "bad" } });
            store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new string[] { "foo1", "foo2" } });
            store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "bad", Scopes = new string[] { "bad" } });
            store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new string[] { "foo1", "foo2", "foo3" } });
            store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "bad", Scopes = new string[] { "bad" } });
            
            store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "bar1" } });
            store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new string[] { "bar1", "bar2" } });
            store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new string[] { "bar1", "bar2", "bar3" } });

            var result = subject.LoadAllAsync("sub").Result;
            Assert.AreEqual(3, result.Count());

            var c1 = result.Single(x => x.ClientId == "client1");
            Assert.AreEqual("sub", c1.Subject);
            CollectionAssert.AreEquivalent(new string[] { "foo1", "bar1" }, c1.Scopes.ToArray());

            var c2 = result.Single(x => x.ClientId == "client2");
            Assert.AreEqual("sub", c1.Subject);
            CollectionAssert.AreEquivalent(new string[] { "foo1", "bar1", "foo2", "bar2" }, c2.Scopes.ToArray());
            
            var c3 = result.Single(x => x.ClientId == "client3");
            Assert.AreEqual("sub", c1.Subject);
            CollectionAssert.AreEquivalent(new string[] { "foo1", "bar1", "foo2", "bar2", "foo3", "bar3" }, c3.Scopes.ToArray());
        }

        [TestMethod]
        public void RevokeAsync_DeletesInAllStores()
        {
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "foo1" } });
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new string[] { "bad" } });

            store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "bar1" } });
            store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new string[] { "bar1", "bar2" } });
            store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new string[] { "bar1", "bar2", "bar3" } });

            subject.RevokeAsync("sub", "client1").Wait();
            Assert.AreEqual(0, store1.LoadAllAsync("sub").Result.Count());
            Assert.AreEqual(2, store2.LoadAllAsync("sub").Result.Count());
        }
    }
}
