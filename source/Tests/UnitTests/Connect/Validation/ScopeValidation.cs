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

            Assert.Null(scopes);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Sorting()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("scope3 scope2 scope1");
            
            Assert.Equal(scopes.Count, 3);

            Assert.Equal(scopes[0], "scope1");
            Assert.Equal(scopes[1], "scope2");
            Assert.Equal(scopes[2], "scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Extra_Spaces()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("   scope3     scope2     scope1   ");

            Assert.Equal(scopes.Count, 3);

            Assert.Equal(scopes[0], "scope1");
            Assert.Equal(scopes[1], "scope2");
            Assert.Equal(scopes[2], "scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Duplicate_Scope()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("scope2 scope1 scope2");

            Assert.Equal(scopes.Count, 2);

            Assert.Equal(scopes[0], "scope1");
            Assert.Equal(scopes[1], "scope2");
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Valid()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.True(result);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Invalid_Scope()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2 unknown");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.False(result);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Disabled_Scope()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2 disabled");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.False(result);
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Unrestricted_Client()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesAllowed(_unrestrictedClient, scopes);

            Assert.True(result);
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Restricted_Client()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid resource1");

            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            Assert.True(result);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_Scopes()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            Assert.False(result);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Contains_Resource_and_Identity_Scopes()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.True(result);
            Assert.True(validator.ContainsOpenIdScopes);
            Assert.True(validator.ContainsResourceScopes);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Contains_Resource_Scopes_Only()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("resource1 resource2");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.True(result);
            Assert.False(validator.ContainsOpenIdScopes);
            Assert.True(validator.ContainsResourceScopes);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Contains_Identity_Scopes_Only()
        {
            var validator = new ScopeValidator();
            var scopes = validator.ParseScopes("openid email");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.True(result);
            Assert.True(validator.ContainsOpenIdScopes);
            Assert.False(validator.ContainsResourceScopes);
        }
    }
}