using System.Security.Claims;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Host.Config
{
    public class Factory
    {
        public static IdentityServerServiceFactory Create(
                    string issuerUri, string siteName)
        {
            var users = new InMemoryUser[]
            {
                new InMemoryUser{Subject = "alice", Username = "alice", Password = "alice", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                    }
                },
                new InMemoryUser{Subject = "bob", Username = "bob", Password = "bob", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                    }
                },
            };
            
            var userSvc = new InMemoryUserService(users);
            var settings = new Settings(issuerUri, siteName);
            var scopes = new InMemoryScopeService(Scopes.Get());
            var clients = new InMemoryClientService(Clients.Get());
            
            var fact = new IdentityServerServiceFactory
            {
                UserService = Registration.RegisterFactory<IUserService>(() => userSvc),
                CoreSettings = Registration.RegisterFactory<CoreSettings>(() => settings),
                ScopeService = Registration.RegisterFactory<IScopeService>(() => scopes),
                ClientService = Registration.RegisterFactory<IClientService>(() => clients)
            };

            return fact;
        }
    }
}