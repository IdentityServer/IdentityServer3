using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Tests
{
    public class TestFactory
    {
        public static IdentityServerServiceFactory Create(
                    string issuerUri, string siteName, string publicHostAddress = "")
        {
            var settings = new TestSettings(issuerUri, siteName, publicHostAddress);
            var scopes = new InMemoryScopeService(Scopes.Get());
            var clients = new InMemoryClientService(TestClients.Get());
            
            var fact = new IdentityServerServiceFactory
            {
                //UserService = Registration.RegisterFactory<IUserService>(() => userSvc),
                CoreSettings = Registration.RegisterFactory<CoreSettings>(() => settings),
                ScopeService = Registration.RegisterFactory<IScopeService>(() => scopes),
                ClientService = Registration.RegisterFactory<IClientService>(() => clients)
            };

            return fact;
        }

        public class Users
        {
            public static IEnumerable<InMemoryUser> Get()
            {
                return new InMemoryUser[]
                {
                    new InMemoryUser{Subject = "818727", Username = "alice", Password = "alice", 
                        Claims = new Claim[]
                        {
                            new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                            new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                        }
                    },
                    new InMemoryUser{Subject = "88421113", Username = "bob", Password = "bob", 
                        Claims = new Claim[]
                        {
                            new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                            new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                        }
                    },
                };
            }
        }

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
}
