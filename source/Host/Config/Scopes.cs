using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Host.Config
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new[]
                {
                    Scope.OpenId,
                    Scope.Profile,
                    Scope.Email,
                    Scope.OfflineAccess,

                    new Scope
                    {
                        Name = "roles",
                        DisplayName = "Roles",
                        Description = "Your organizational roles",
                        Type = ScopeType.Identity,
                        Claims = new[]
                        {
                            new ScopeClaim("role")
                        }
                    },

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
                        Name = "idmgr",
                        DisplayName = "IdentityManager",
                        Type = ScopeType.Resource,
                        Emphasize = true,
                        Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim("name"),
                            new ScopeClaim("role")
                        }
                    }
                };
        }
    }
}