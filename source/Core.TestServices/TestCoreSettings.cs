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
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core;
using System;
using System.IO;

namespace Thinktecture.IdentityServer.TestServices
{
    public class TestCoreSettings : ICoreSettings
    {
        private string _issuerUri;
        private string _siteName;
        private X509Certificate2 _certificate;
        private string _publicHostAddress;

        public TestCoreSettings(string issuerUri, string siteName, string publicHostAddress)
        {
            _issuerUri = issuerUri;
            _siteName = siteName;
            _publicHostAddress = publicHostAddress;

            var assembly = this.GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream("Thinktecture.IdentityServer.TestServices.idsrv3test.pfx"))
            {
                _certificate = new X509Certificate2(ReadStream(stream), "idsrv3test");
            }
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return Task.FromResult((from c in TestClients.Get()
                                    where c.ClientId == clientId &&
                                          c.Enabled == true
                                    select c).SingleOrDefault());
        }

        public Task<IEnumerable<Scope>> GetScopesAsync()
        {
            return Task.FromResult(TestScopes.Get());
        }

        public X509Certificate2 GetSigningCertificate()
        {
            if (_certificate == null)
            {
                throw new InvalidOperationException("No certificate specified.");
            }

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

        private static byte[] ReadStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}