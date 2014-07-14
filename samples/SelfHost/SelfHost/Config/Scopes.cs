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
            return new Scope[]
            {
                new Scope
                {
                    Name = Constants.StandardScopes.OpenId, 
                    DisplayName = "Your user identifier",
                    Required = true,
                    IsOpenIdScope = true,
                    Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim
                            {
                                AlwaysIncludeInIdToken = true,
                                Name = "sub",
                                Description = "subject identifier"
                            }
                        }
                 },
                 new Scope
                 {
                    Name = Constants.StandardScopes.Profile, 
                    DisplayName = "Basic profile",
                    Description = "Your basic user profile information (first name, last name, etc.). This is a really long string to see what the UI look like when someone puts in too much stuff here. I know this is not what we really want, but this is just test data (for now). KThxBye.",
                    IsOpenIdScope = true,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(x=>new ScopeClaim{Name = x, Description = x}))
                },
                new Scope
                {
                    Name = Constants.StandardScopes.Email, 
                    DisplayName = "Your email address",
                    IsOpenIdScope = true,
                    Emphasize = true,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim
                        {
                            Name = "email",
                            Description = "email address",
                        },
                        new ScopeClaim
                        {
                            Name = "email_verified",
                            Description = "email is verified",
                        }
                    }
                },
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
                },
                new Scope
                {
                    Name = "forbidden",
                    DisplayName = "Forbidden scope",
                    Emphasize = true
                }
             };
        }
    }
}