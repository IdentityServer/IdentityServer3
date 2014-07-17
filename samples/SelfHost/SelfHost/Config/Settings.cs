using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Host.Config
{
    public class Settings : CoreSettings
    {
        private string _issuerUri;
        private string _siteName;
        private X509Certificate2 _certificate;
        private string _publicHostAddress;

        public Settings(string issuerUri, string siteName)
        {
            _issuerUri = issuerUri;
            _siteName = siteName;

            var assembly = this.GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream("SelfHost.Config.idsrv3test.pfx"))
            {
                _certificate = new X509Certificate2(ReadStream(stream), "idsrv3test");
            }
        }

        public override X509Certificate2 SigningCertificate
        {
            get
            {
                if (_certificate == null)
                {
                    throw new InvalidOperationException("No certificate specified.");
                }

                return _certificate;
            }
        }

        public override string IssuerUri
        {
            get { return _issuerUri; }
        }

        public override string SiteName
        {
            get { return _siteName; }
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