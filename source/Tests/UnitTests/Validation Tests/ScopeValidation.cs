using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Connect.Models;
using System.Collections.Generic;

namespace UnitTests.Validation_Tests
{
    [TestClass]
    public class ScopeValidation
    {
        const string Category = "Scope Validation";

        ILogger _logger = new DebugLogger();
        List<Scope> _allScopes = new List<Scope>
            {
                new Scope
                {
                    Name = "openid",
                    IsOpenIdScope = true
                },
                new Scope
                {
                    Name = "email",
                    IsOpenIdScope = true
                },
                new Scope
                {
                    Name = "resource1",
                    IsOpenIdScope = false
                },
                new Scope
                {
                    Name = "resource2",
                    IsOpenIdScope = false
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
                "resource1"
            }
        };

        [TestMethod]
        [TestCategory(Category)]
        public void Parse_Scopes_with_Empty_Scope_List()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("");

            Assert.IsNull(scopes);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Parse_Scopes_with_Sorting()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("scope3 scope2 scope1");
            
            Assert.AreEqual(scopes.Count, 3);

            Assert.AreEqual(scopes[0], "scope1");
            Assert.AreEqual(scopes[1], "scope2");
            Assert.AreEqual(scopes[2], "scope3");
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Parse_Scopes_with_Extra_Spaces()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("   scope3     scope2     scope1   ");

            Assert.AreEqual(scopes.Count, 3);

            Assert.AreEqual(scopes[0], "scope1");
            Assert.AreEqual(scopes[1], "scope2");
            Assert.AreEqual(scopes[2], "scope3");
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Parse_Scopes_with_Duplicate_Scope()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("scope2 scope1 scope2");

            Assert.AreEqual(scopes.Count, 2);

            Assert.AreEqual(scopes[0], "scope1");
            Assert.AreEqual(scopes[1], "scope2");
        }

        [TestMethod]
        [TestCategory(Category)]
        public void All_Scopes_Valid()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Invalid_Scope()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("openid email resource1 resource2 unknown");

            var result = validator.AreScopesValid(scopes, _allScopes);

            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void All_Scopes_Allowed_For_Unrestricted_Client()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesAllowed(_unrestrictedClient, scopes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void All_Scopes_Allowed_For_Restricted_Client()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("openid resource1");

            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory(Category)]
        public void Restricted_Scopes()
        {
            var validator = new ScopeValidator(_logger);
            var scopes = validator.ParseScopes("openid email resource1 resource2");

            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            Assert.IsFalse(result);
        }
    }
}