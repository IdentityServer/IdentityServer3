using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.TestServices
{
    public class TestCoreSettings : ICoreSettings
    {
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return Task.FromResult((from c in TestClients.Get()
                                    where c.ClientId == clientId
                                    select c).SingleOrDefault());
        }

        public Task<IEnumerable<Scope>> GetScopesAsync()
        {
            return Task.FromResult(TestScopes.Get());
        }

        public X509Certificate2 GetSigningCertificate()
        {
            return X509.LocalMachine.My.SubjectDistinguishedName.Find("CN=sts", false).First();
        }

        public string GetIssuerUri()
        {
            return "https://idsrv3.com";
        }

        public string GetSiteName()
        {
            return "IdentityServer v3 - alpha1";
        }

        public string GetPublicHost()
        {
            return "http://localhost:3333";
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