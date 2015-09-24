/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using FluentAssertions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using System.Linq;
using Xunit;

namespace IdentityServer3.Tests.Services.Default
{
    public class AggregatePermissionsStoreTests
    {
        AggregatePermissionsStore subject;
        InMemoryConsentStore store1;
        InMemoryConsentStore store2;

        public AggregatePermissionsStoreTests()
        {
            store1 = new InMemoryConsentStore();
            store2 = new InMemoryConsentStore();
            subject = new AggregatePermissionsStore(store1, store2);
        }

        [Fact]
        public void LoadAllAsync_EmptyStores_ReturnsEmptyConsentCollection()
        {
            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(0);
        }

        [Fact]
        public void LoadAllAsync_OnlyOneStoreHasConsent_ReturnsSameConsent()
        {
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new [] { "foo", "bar" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveSameConsent_ReturnsSameConsent()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new [] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new [] { "foo", "bar" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveOverlappingConsent_ReturnsCorrectUnion()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "bar", "baz" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar", "baz" });
        }

        [Fact]
        public void LoadAllAsync_BothStoresHaveDifferentConsent_ReturnsCorrectUnion()
        {
            store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });
            store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "quux", "baz" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar", "baz", "quux" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveMultipleClientConsent_ReturnsCorrectConsent()
        {
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "foo1" } });
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new[] { "bad" } });
            store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new[] { "foo1", "foo2" } });
            store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "bad", Scopes = new[] { "bad" } });
            store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new[] { "foo1", "foo2", "foo3" } });
            store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "bad", Scopes = new[] { "bad" } });
            
            store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "bar1" } });
            store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new[] { "bar1", "bar2" } });
            store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new[] { "bar1", "bar2", "bar3" } });

            var result = subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(3);

            var c1 = result.Single(x => x.ClientId == "client1");
            c1.Subject.Should().Be("sub");
            c1.Scopes.ShouldAllBeEquivalentTo(new[] { "foo1", "bar1" });

            var c2 = result.Single(x => x.ClientId == "client2");
            c1.Subject.Should().Be("sub");
            c2.Scopes.ShouldAllBeEquivalentTo(new[] { "foo1", "bar1", "foo2", "bar2" });
            
            var c3 = result.Single(x => x.ClientId == "client3");
            c1.Subject.Should().Be("sub");
            c3.Scopes.ShouldAllBeEquivalentTo(new[] { "foo1", "bar1", "foo2", "bar2", "foo3", "bar3" });
        }

        [Fact]
        public void RevokeAsync_DeletesInAllStores()
        {
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "foo1" } });
            store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new string[] { "bad" } });

            store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new string[] { "bar1" } });
            store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new string[] { "bar1", "bar2" } });
            store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new string[] { "bar1", "bar2", "bar3" } });

            subject.RevokeAsync("sub", "client1").Wait();
            store1.LoadAllAsync("sub").Result.Count().Should().Be(0);
            store2.LoadAllAsync("sub").Result.Count().Should().Be(2);
        }
    }
}
