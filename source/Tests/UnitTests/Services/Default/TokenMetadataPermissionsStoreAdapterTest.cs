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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    
    public class TokenMetadataPermissionsStoreAdapterTest
    {
        readonly List<ITokenMetadata> _tokens;
        readonly Func<string, Task<IEnumerable<ITokenMetadata>>> _get;

        readonly Func<string, string, Task> _delete;
        string _subjectDeleted;
        string _clientDeleted;

        readonly TokenMetadataPermissionsStoreAdapter _subject;

        
        public TokenMetadataPermissionsStoreAdapterTest()
        {
            _tokens = new List<ITokenMetadata>();
            _get = s => Task.FromResult(_tokens.AsEnumerable());
            _delete = (subject, client) =>
            {
                _subjectDeleted = subject;
                _clientDeleted = client;
                return Task.FromResult(0);
            };
            _subject = new TokenMetadataPermissionsStoreAdapter(_get, _delete);
        }

        class TokenMeta : ITokenMetadata
        {
            public TokenMeta(string sub, string client, IEnumerable<string> scopes)
            {
                SubjectId = sub;
                ClientId = client;
                Scopes = scopes;
            }
            public string SubjectId {get; private set;}
            public string ClientId {get; private set;}
            public IEnumerable<string> Scopes {get; private set;}
        }

        [Fact]
        public void LoadAllAsync_CallsGet_MapsResultsToConsent()
        {
            _tokens.Add(new TokenMeta("sub", "client1", new[] { "foo", "bar" }));
            _tokens.Add(new TokenMeta("sub", "client2", new[] { "baz", "quux" }));

            var result = _subject.LoadAllAsync("sub").Result;
            result.Count().Should().Be(2);

            var c1 = result.Single(x=>x.ClientId == "client1");
            c1.Subject.Should().Be("sub");
            c1.Scopes.ShouldAllBeEquivalentTo(new[] { "foo", "bar" });

            var c2 = result.Single(x=>x.ClientId == "client2");
            c2.Subject.Should().Be("sub");
            c2.Scopes.ShouldAllBeEquivalentTo(new[] { "baz", "quux" });
        }

        [Fact]
        public void RevokeAsync_CallsRevoke()
        {
            _subject.RevokeAsync("sub34", "client12").Wait();
            _subjectDeleted.Should().Be("sub34");
            _clientDeleted.Should().Be("client12");
        }
    }
}
