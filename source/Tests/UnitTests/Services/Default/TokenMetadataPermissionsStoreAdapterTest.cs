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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Services.Default
{
    
    public class TokenMetadataPermissionsStoreAdapterTest
    {
        List<ITokenMetadata> tokens;
        Func<string, Task<IEnumerable<ITokenMetadata>>> get;
        
        Func<string, string, Task> delete;
        string subjectDeleted;
        string clientDeleted;

        TokenMetadataPermissionsStoreAdapter subject;

        
        public TokenMetadataPermissionsStoreAdapterTest()
        {
            tokens = new List<ITokenMetadata>();
            get = s => Task.FromResult(tokens.AsEnumerable());
            delete = (subject, client) =>
            {
                subjectDeleted = subject;
                clientDeleted = client;
                return Task.FromResult(0);
            };
            this.subject = new TokenMetadataPermissionsStoreAdapter(get, delete);
        }

        class TokenMeta : ITokenMetadata
        {
            public TokenMeta(string sub, string client, IEnumerable<string> scopes)
            {
                SubjectId = sub;
                ClientId = client;
                Scopes = scopes;
            }
            public string SubjectId {get; set;}
            public string ClientId {get; set;}
            public IEnumerable<string> Scopes {get; set;}
        }

        [Fact]
        public void LoadAllAsync_CallsGet_MapsResultsToConsent()
        {
            tokens.Add(new TokenMeta("sub", "client1", new string[] { "foo", "bar" }));
            tokens.Add(new TokenMeta("sub", "client2", new string[] { "baz", "quux" }));

            var result = subject.LoadAllAsync("sub").Result;
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
            subject.RevokeAsync("sub34", "client12").Wait();
            subjectDeleted.Should().Be("sub34");
            clientDeleted.Should().Be("client12");
        }
    }
}
