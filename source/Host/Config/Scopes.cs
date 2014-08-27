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
                        Name = "read",
                        DisplayName = "Read data",
                        IsOpenIdScope = false,
                        Emphasize = false,
                    },
                    new Scope
                    {
                        Name = "write",
                        DisplayName = "Write data",
                        IsOpenIdScope = false,
                        Emphasize = true,
                    }
                };
        }
    }
}