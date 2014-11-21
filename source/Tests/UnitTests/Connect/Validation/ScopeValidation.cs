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

using System.Collections.Generic;
using FluentAssertions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Connect.Validation.Scopes
{
    
    public class ScopeValidation
    {
        const string Category = "Scope Validation";

        List<Scope> _allScopes = new List<Scope>
            {
                new Scope
                {
                    Name = "openid",
                    Type = ScopeType.Identity
                },
                new Scope
                {
                    Name = "email",
                    Type = ScopeType.Identity
                },
                new Scope
                {
                    Name = "resource1",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "resource2",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "disabled",
                    Enabled = false,
                    Type = ScopeType.Resource
                },
            };

        Client _unrestrictedClient = new Client
            {
                ClientId = "unrestricted"
            };

        Client _restrictedClient = new Client
            {
                ClientId = "restricted",
            
                ScopeRestrictions = new List<string>
                {
                    "openid",
                    "resource1",
                    "disabled"
                }
            };

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Empty_Scope_List()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("");

            scopes.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Sorting()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("scope3 scope2 scope1");

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Extra_Spaces()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("   scope3     scope2     scope1   ");

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Duplicate_Scope()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("scope2 scope1 scope2");

            scopes.Count.Should().Be(2);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Valid()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesValid(scopes, _allScopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Invalid_Scope()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2 unknown");

            var result = validator.AreScopesValid(scopes, _allScopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Disabled_Scope()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2 disabled");

            var result = validator.AreScopesValid(scopes, _allScopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Unrestricted_Client()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesAllowed(_unrestrictedClient, scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Restricted_Client()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid resource1");

            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_Scopes()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Contains_Resource_and_Identity_Scopes()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesValid(scopes, _allScopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Contains_Resource_Scopes_Only()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("resource1 resource2");

            var result = validator.AreScopesValid(scopes, _allScopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeFalse();
            validator.ContainsResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Contains_Identity_Scopes_Only()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email");

            var result = validator.AreScopesValid(scopes, _allScopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsResourceScopes.Should().BeFalse();
        }
    }
}