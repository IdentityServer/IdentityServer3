using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Tests.Plumbing
{
    class TestScopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new Scope[]
            {
                Scope.OpenId,
                Scope.Profile,
                Scope.OfflineAccess,

                new Scope
                {
                    Name = "resource",
                    Description = "resource scope",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "resource2",
                    Description = "resource scope",
                    Type = ScopeType.Resource
                },
             };
        }
    }
}