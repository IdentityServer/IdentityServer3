using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Tests
{
    public class TestScopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new Scope[]
            {
                Scope.OpenId,
                Scope.Profile,
                Scope.Email,

                new Scope
                {
                    Name = "read",
                    DisplayName = "Read data",
                    Type = ScopeType.Resource,
                    Emphasize = false,
                },
                new Scope
                {
                    Name = "write",
                    DisplayName = "Write data",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                },
                new Scope
                {
                    Name = "forbidden",
                    Type = ScopeType.Resource,
                    DisplayName = "Forbidden scope",
                    Emphasize = true
                }
             };
        }
    }
}
