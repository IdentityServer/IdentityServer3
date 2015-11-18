using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                }
            };
        }
    }
}
