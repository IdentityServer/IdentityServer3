using System.IO;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Tests
{
    class TestIdentityServerOptions
    {
        public static IdentityServerOptions Create()
        {
            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "Thinktecture IdentityServer v3 - test",
                DataProtector = new NoDataProtector(),
            };

            var assembly = typeof(TestIdentityServerOptions).Assembly;
            using (var stream = assembly.GetManifestResourceStream("Thinktecture.IdentityServer.Tests.idsrv3test.pfx"))
            {
                options.SigningCertificate = new X509Certificate2(ReadStream(stream), "idsrv3test");
            }

            return options;
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
