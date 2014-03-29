using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests.Plumbing
{
    class TestSettings : ICoreSettings
    {
        List<Client> _clients = new List<Client>
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
                        new Uri("https://server/cb"),
                    },

                    AuthorizationCodeLifetime = 60
                },
                new Client
                {
                    ClientName = "Implicit Client",
                    ClientId = "implicitclient",
                    ClientSecret = "secret",
                    Flow = Flows.Implicit,
                    ApplicationType = ApplicationTypes.Native,
                    RequireConsent = false,
                    
                    RedirectUris = new List<Uri>
                    {
                        new Uri("oob://implicit/cb")
                    },
                },
                new Client
                {
                    ClientName = "Code Client with Scope Restrictions",
                    ClientId = "codeclient_restricted",
                    ClientSecret = "secret",
                    Flow = Flows.Code,
                    ApplicationType = ApplicationTypes.Web,
                    RequireConsent = false,

                    ScopeRestrictions = new List<string>
                    {
                        "openid"
                    },
                    
                    RedirectUris = new List<Uri>
                    {
                        new Uri("https://server/cb"),
                    },
                },
                new Client
                {
                    ClientName = "Client Credentials Client",
                    ClientId = "client",
                    ClientSecret = "secret",
                    Flow = Flows.ClientCredentials,
                },
                new Client
                {
                    ClientName = "Client Credentials Client (restricted)",
                    ClientId = "client_restricted",
                    ClientSecret = "secret",
                    Flow = Flows.ClientCredentials,

                    ScopeRestrictions = new List<string>
                    {
                        "resource"
                    },       
                },
                new Client
                {
                    ClientName = "Resource Owner Client",
                    ClientId = "roclient",
                    ClientSecret = "secret",
                    Flow = Flows.ResourceOwner,
                },
                new Client
                {
                    ClientName = "Resource Owner Client (restricted)",
                    ClientId = "roclient_restricted",
                    ClientSecret = "secret",
                    Flow = Flows.ResourceOwner,

                    ScopeRestrictions = new List<string>
                    {
                        "resource"
                    },       
                },
                new Client
                {
                    ClientName = "Assertion Flow Client",
                    ClientId = "assertionclient",
                    ClientSecret = "secret",
                    Flow = Flows.Assertion,
                }
        };

        public Task<IEnumerable<Scope>> GetScopesAsync()
        {
            return Task.FromResult<IEnumerable<Scope>>(new Scope[]
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
             });
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return Task.FromResult(_clients.FirstOrDefault(c => c.ClientId == clientId));
        }

        public bool RequiresConsent(string clientId, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            return false;
        }

        public X509Certificate2 GetSigningCertificate()
        {
            throw new NotImplementedException();
        }

        public string GetIssuerUri()
        {
            throw new NotImplementedException();
        }

        public string GetSiteName()
        {
            throw new NotImplementedException();
        }

        public InternalProtectionSettings GetInternalProtectionSettings()
        {
            throw new NotImplementedException();
        }


        public string GetPublicHost()
        {
            throw new NotImplementedException();
        }


        public bool RequiresConsent(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            throw new NotImplementedException();
        }
    }
}