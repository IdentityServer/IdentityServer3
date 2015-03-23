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

using System.Linq;
using FluentAssertions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    public class AggregatePermissionsStoreTests
    {
        readonly AggregatePermissionsStore _subject;
        readonly InMemoryConsentStore _store1;
        readonly InMemoryConsentStore _store2;

        public AggregatePermissionsStoreTests()
        {
            _store1 = new InMemoryConsentStore();
            _store2 = new InMemoryConsentStore();
            _subject = new AggregatePermissionsStore(_store1, _store2);
        }

        [Fact]
        public void LoadAllAsync_EmptyStores_ReturnsEmptyConsentCollection()
        {
            var result = _subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(0);
        }

        [Fact]
        public void LoadAllAsync_OnlyOneStoreHasConsent_ReturnsSameConsent()
        {
            _store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });

            var result = _subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new [] { "foo", "bar" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveSameConsent_ReturnsSameConsent()
        {
            _store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new [] { "foo", "bar" } });
            _store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });

            var result = _subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new [] { "foo", "bar" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveOverlappingConsent_ReturnsCorrectUnion()
        {
            _store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });
            _store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "bar", "baz" } });

            var result = _subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar", "baz" });
        }

        [Fact]
        public void LoadAllAsync_BothStoresHaveDifferentConsent_ReturnsCorrectUnion()
        {
            _store1.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "foo", "bar" } });
            _store2.UpdateAsync(new Consent { ClientId = "client", Subject = "sub", Scopes = new[] { "quux", "baz" } });

            var result = _subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(1);
            var consent = result.First();
            consent.Subject.Should().Be("sub");
            consent.ClientId.Should().Be("client");
            consent.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar", "baz", "quux" });
        }
        
        [Fact]
        public void LoadAllAsync_StoresHaveMultipleClientConsent_ReturnsCorrectConsent()
        {
            _store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "foo1" } });
            _store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new[] { "bad" } });
            _store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new[] { "foo1", "foo2" } });
            _store1.UpdateAsync(new Consent { ClientId = "client2", Subject = "bad", Scopes = new[] { "bad" } });
            _store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new[] { "foo1", "foo2", "foo3" } });
            _store1.UpdateAsync(new Consent { ClientId = "client3", Subject = "bad", Scopes = new[] { "bad" } });
            
            _store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "bar1" } });
            _store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new[] { "bar1", "bar2" } });
            _store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new[] { "bar1", "bar2", "bar3" } });

            var result = _subject.LoadAllAsync("sub").Result;
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
            _store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "foo1" } });
            _store1.UpdateAsync(new Consent { ClientId = "client1", Subject = "bad", Scopes = new[] { "bad" } });

            _store2.UpdateAsync(new Consent { ClientId = "client1", Subject = "sub", Scopes = new[] { "bar1" } });
            _store2.UpdateAsync(new Consent { ClientId = "client2", Subject = "sub", Scopes = new[] { "bar1", "bar2" } });
            _store2.UpdateAsync(new Consent { ClientId = "client3", Subject = "sub", Scopes = new[] { "bar1", "bar2", "bar3" } });

            _subject.RevokeAsync("sub", "client1").Wait();
            _store1.LoadAllAsync("sub").Result.Count().Should().Be(0);
            _store2.LoadAllAsync("sub").Result.Count().Should().Be(2);
        }
    }
}
