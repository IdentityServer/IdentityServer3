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

using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    public class DefaultConsentServiceTests
    {
        readonly DefaultConsentService _subject;
        readonly InMemoryConsentStore _store;
        readonly ClaimsPrincipal _user;
        readonly Client _client;
        readonly List<string> _scopes;
        
        public DefaultConsentServiceTests()
        {
            _scopes = new List<string> { "read", "write" };
            _client = new Client {ClientId = "client", AllowRememberConsent = true, RequireConsent = true};
            _user = new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(Constants.ClaimTypes.SUBJECT, "123")}, "password"));
            _store = new InMemoryConsentStore();
            _subject = new DefaultConsentService(_store);
        }

        [Fact]
        public void RequiresConsentAsync_NoPriorConsentGiven_ReturnsTrue()
        {
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_ReturnsFalse()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_ScopesInDifferentOrder_ReturnsFalse()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            _scopes.Reverse();
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_MoreScopesRequested_ReturnsTrue()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            _scopes.Add("query");
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_FewerScopesRequested_ReturnsFalse()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            _scopes.RemoveAt(0);
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeFalse();
        }
        
        [Fact]
        public void RequiresConsentAsync_PriorConsentGiven_NoScopesRequested_ReturnsFalse()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            _scopes.Clear();
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_ClientDoesNotRequireConsent_ReturnsFalse()
        {
            _client.RequireConsent = false;
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresConsentAsync_ClientDoesNotAllowRememberConsent_ReturnsTrue()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            _client.AllowRememberConsent = false;
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateConsentAsync_ConsentGiven_ConsentNoLongerRequired()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            var result =  _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeFalse();
        }

        [Fact]
        public void UpdateConsentAsync_ClientDoesNotAllowRememberConsent_ConsentStillRequired()
        {
            _client.AllowRememberConsent = false;
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateConsentAsync_PriorConsentGiven_NullScopes_ConsentNowRequired()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            _subject.UpdateConsentAsync(_client, _user, null).Wait();
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateConsentAsync_PriorConsentGiven_EmptyScopeCollection_ConsentNowRequired()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            _subject.UpdateConsentAsync(_client, _user, new string[0]).Wait();
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeTrue();
        }
        
        [Fact]
        public void UpdateConsentAsync_ChangeConsent_OldConsentNotAllowed()
        {
            _subject.UpdateConsentAsync(_client, _user, _scopes).Wait();
            var newConsent = new[] { "foo", "bar" };
            _subject.UpdateConsentAsync(_client, _user, newConsent).Wait();
            var result = _subject.RequiresConsentAsync(_client, _user, _scopes).Result;
            result.Should().BeTrue();
        }
    }
}
