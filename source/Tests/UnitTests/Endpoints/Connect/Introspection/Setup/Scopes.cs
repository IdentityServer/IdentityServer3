using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdentityServer3.Tests.Endpoints.Connect.Introspection.Setup
{
    class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "api1",
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                },
                new Scope
                {
                    Name = "api2",
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    }
                },
                new Scope
                {
                    Name = "unrestricted.api",
                    AllowUnrestrictedIntrospection = true,

                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    }
                }
            };
        }
    }
}