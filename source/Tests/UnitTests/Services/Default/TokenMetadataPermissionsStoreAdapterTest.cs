using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    [TestClass]
    public class TokenMetadataPermissionsStoreAdapterTest
    {
        List<ITokenMetadata> tokens;
        Func<string, Task<IEnumerable<ITokenMetadata>>> get;
        
        Func<string, string, Task> delete;
        string subjectDeleted;
        string clientDeleted;

        TokenMetadataPermissionsStoreAdapter subject;

        [TestInitialize]
        public void Init()
        {
            tokens = new List<ITokenMetadata>();
            get = s => Task.FromResult(tokens.AsEnumerable());
            delete = (subject, client) =>
            {
                this.subjectDeleted = subject;
                this.clientDeleted = client;
                return Task.FromResult(0);
            };
            this.subject = new TokenMetadataPermissionsStoreAdapter(get, delete);
        }

        class TokenMeta : ITokenMetadata
        {
            public TokenMeta(string sub, string client, IEnumerable<string> scopes)
            {
                this.SubjectId = sub;
                this.ClientId = client;
                this.Scopes = scopes;
            }
            public string SubjectId {get; set;}
            public string ClientId {get; set;}
            public IEnumerable<string> Scopes {get; set;}
        }

        [TestMethod]
        public void LoadAllAsync_CallsGet_MapsResultsToConsent()
        {
            tokens.Add(new TokenMeta("sub", "client1", new string[] { "foo", "bar" }));
            tokens.Add(new TokenMeta("sub", "client2", new string[] { "baz", "quux" }));

            var result = this.subject.LoadAllAsync("sub").Result;
            Assert.AreEqual(2, result.Count());
            
            var c1 = result.Single(x=>x.ClientId == "client1");
            Assert.AreEqual("sub", c1.Subject);
            CollectionAssert.AreEquivalent(new string[] { "foo", "bar" }, c1.Scopes.ToArray());

            var c2 = result.Single(x=>x.ClientId == "client2");
            Assert.AreEqual("sub", c2.Subject);
            CollectionAssert.AreEquivalent(new string[] { "baz", "quux" }, c2.Scopes.ToArray());
        }

        [TestMethod]
        public void RevokeAsync_CallsRevoke()
        {
            subject.RevokeAsync("sub34", "client12").Wait();
            Assert.AreEqual("sub34", this.subjectDeleted);
            Assert.AreEqual("client12", this.clientDeleted);
        }
    }
}
