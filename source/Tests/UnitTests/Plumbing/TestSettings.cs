/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Configuration;

namespace UnitTests.Plumbing
{
    class TestSettings : CoreSettings
    {
        static X509Certificate2 _certificate;

        static TestSettings()
        {
            var assembly = typeof(TestSettings).Assembly;
            using (var stream = assembly.GetManifestResourceStream("Thinktecture.IdentityServer.Tests.Plumbing.idsrv3test.pfx"))
            {
                _certificate = new X509Certificate2(ReadStream(stream));
            }
        }

        public override X509Certificate2 SigningCertificate
        {
            get { return _certificate; }
        }

        public override string IssuerUri
        {
            get { return "https://idsrv3.test"; }
        }

        public override string SiteName
        {
            get { throw new NotImplementedException(); }
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