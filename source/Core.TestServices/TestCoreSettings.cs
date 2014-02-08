using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityModel;

namespace Thinktecture.IdentityServer.Core.Connect.TestServices
{
    public class TestCoreSettings : ICoreSettings
    {
        private static readonly List<Client> _clients;

        static TestCoreSettings()
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
                        new Uri("https://localhost/cb"),
                        new Uri("https://web.local/oidccode/oidccallback")
                    },
                    
                    ScopeRestrictions = new List<string>
                    { 
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.OpenId,
                        "resource1"
                    },

                    
                    IdentityTokenSigningKeyType = SigningKeyTypes.ClientSecret,
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
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.OpenId
                    },

                    IdentityTokenSigningKeyType = SigningKeyTypes.ClientSecret,
                    SubjectType = SubjectTypes.Global,
                    
                    IdentityTokenLifetime = 360,
                    AccessTokenLifetime = 360,
                }
            };
        }

        public Client FindClientById(string clientId)
        {
            return (from c in _clients
                    where c.ClientId == clientId
                    select c).SingleOrDefault();
        }

        public IEnumerable<Scope> GetScopes()
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
                    Name = Constants.StandardScopes.Email, 
                    Description = "Email",
                    IsOpenIdScope = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(x=>new ScopeClaim{Name = x}))
                },
                new Scope
                {
                    Name = "resource1",
                    Description = "resource scope 1",
                    IsOpenIdScope = false
                },
                new Scope
                {
                    Name = "resource2",
                    Description = "resource scope 2",
                    IsOpenIdScope = false
                }
             };
        }

        public bool RequiresConsent(string clientId, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            return false;
        }

        public System.Security.Cryptography.X509Certificates.X509Certificate2 GetSigningCertificate()
        {
            return X509.LocalMachine.My.SubjectDistinguishedName.Find("CN=sts", false).First();
        }

        public string GetIssuerUri()
        {
            return "https://idsrv3.com";
        }


        public string GetSiteName()
        {
            return "tt.IdSrv 3";
        }


        public InternalProtectionSettings GetInternalProtectionSettings()
        {
            var settings = new InternalProtectionSettings
            {
                Issuer = GetIssuerUri(),
                Audience = "internal",
                SigningKey = "jKhUkbfzz4IqMTo66J6GATNgOWqA38SFNMCo/FR1Yhs=",
                Ttl = 60
            };

            return settings;
        }
    }
}
