using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdentityServer3.Tests.TokenClients
{
    class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Email,
                StandardScopes.OfflineAccess,
                StandardScopes.Address,

                new Scope
                {
                    Name = "api1",
                    Type = ScopeType.Resource,

                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    }
                },
                new Scope
                {
                    Name = "api2",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "api3",
                    Type = ScopeType.Resource
                }
            };
        }
    }
}