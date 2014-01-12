using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Repositories
{
    public class InMemoryClientsRepository : IClientsRepository
    {
        private static readonly List<Client> _clients;

        static InMemoryClientsRepository()
        {
            _clients = new List<Client>
            {
                new Client
                {
                    ClientName = "Code Client",
                    ClientId = "codeclient",
                    ClientSecret = "secret",
                    Flow = Flows.Code,
                    ApplicationType = ApplicationTypes.Web,
                    RequireConsent = false,
                    
                    RedirectUris = new List<Uri>
                    {
                        new Uri("https://localhost/cb")
                    },
                    
                    ScopeRestrictions = new List<string>
                    { 
                        Constants.StandardScopes.Profile 
                    },

                    SigningKeyType = SigningKeyTypes.ClientSecret,
                    SubjectType = SubjectTypes.Global,
                    
                    IdentityTokenLifetime = 360,
                    AccessTokenLifetime = 360,
                },
                new Client
                {
                    ClientName = "Implicit Client",
                    ClientId = "implicitclient",
                    ClientSecret = "secret",
                    Flow = Flows.Implicit,
                    ApplicationType = ApplicationTypes.Web,
                    RequireConsent = false,
                    
                    RedirectUris = new List<Uri>
                    {
                        new Uri("https://localhost/cb")
                    },
                    
                    ScopeRestrictions = new List<string>
                    { 
                        Constants.StandardScopes.Profile 
                    },

                    SigningKeyType = SigningKeyTypes.ClientSecret,
                    SubjectType = SubjectTypes.Global,
                    
                    IdentityTokenLifetime = 360,
                    AccessTokenLifetime = 360,
                }
            };
        }

        public Client FindById(string clientId)
        {
            return (from c in _clients
                    where c.ClientId == clientId
                    select c).SingleOrDefault();
        }
    }
}
