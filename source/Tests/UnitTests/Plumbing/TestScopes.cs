using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Tests.Plumbing
{
    class TestScopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new Scope[]
            {
                new Scope
                 {
                    Name = Constants.StandardScopes.OpenId, 
                    Description = "User identifier",
                    IsOpenIdScope = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.OpenId].Select(x=>new ScopeClaim{Name = x}))
                },
                 new Scope
                 {
                    Name = Constants.StandardScopes.Profile, 
                    Description = "User profile",
                    IsOpenIdScope = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(x=>new ScopeClaim{Name = x}))
                },
                new Scope
                {
                    Name = "resource",
                    Description = "resource scope",
                    IsOpenIdScope = false
                },
                new Scope
                {
                    Name = "resource2",
                    Description = "resource scope",
                    IsOpenIdScope = false
                },
             };
        }
    }
}