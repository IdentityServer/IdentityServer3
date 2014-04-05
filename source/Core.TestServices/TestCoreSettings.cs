/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
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
        private string _issuerUri;
        private string _siteName;
        private X509Certificate2 _certificate;
        private string _publicHostAddress;

        public TestCoreSettings(string issuerUri, string siteName, string certificateName, string publicHostAddress)
        {
            _issuerUri = issuerUri;
            _siteName = siteName;
            _certificate = X509.LocalMachine.My.SubjectDistinguishedName.Find(certificateName, false).First();
            _publicHostAddress = publicHostAddress;
        }

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
            return _certificate;
        }

        public string GetIssuerUri()
        {
            return _issuerUri;
        }

        public string GetSiteName()
        {
            return _siteName;
        }

        public string GetPublicHost()
        {
            return _publicHostAddress;
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