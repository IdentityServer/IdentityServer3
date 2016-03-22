/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace IdentityServer3.Core.Services.Default
{
    internal class X509CertificateFactory
    {
        internal static X509Certificate2
            GenerateCertificate(string subjectName, AsymmetricKeyParameter publicKey)
        {
            var kpgen = new RsaKeyPairGenerator();

            // certificate strength 1024 bits
            kpgen.Init(new KeyGenerationParameters(
                  new SecureRandom(new CryptoApiRandomGenerator()), 1024));

            var gen = new X509V3CertificateGenerator();

            var certName = new X509Name("CN=" + subjectName);
            var serialNo = BigInteger.ProbablePrime(120, new Random());

            gen.SetSerialNumber(serialNo);
            gen.SetSubjectDN(certName);
            gen.SetIssuerDN(certName);
            gen.SetNotAfter(DateTime.Now.AddYears(100));
            gen.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)));
            gen.SetSignatureAlgorithm("SHA1withRSA");
            gen.SetPublicKey(publicKey);

            gen.AddExtension(
                X509Extensions.AuthorityKeyIdentifier.Id,
                false,
                new AuthorityKeyIdentifier(
                    SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey),
                    new GeneralNames(new GeneralName(certName)),
                    serialNo));

            /* 
         1.3.6.1.5.5.7.3.1 - id_kp_serverAuth 
         1.3.6.1.5.5.7.3.2 - id_kp_clientAuth 
         1.3.6.1.5.5.7.3.3 - id_kp_codeSigning 
         1.3.6.1.5.5.7.3.4 - id_kp_emailProtection 
         1.3.6.1.5.5.7.3.5 - id-kp-ipsecEndSystem 
         1.3.6.1.5.5.7.3.6 - id-kp-ipsecTunnel 
         1.3.6.1.5.5.7.3.7 - id-kp-ipsecUser 
         1.3.6.1.5.5.7.3.8 - id_kp_timeStamping 
         1.3.6.1.5.5.7.3.9 - OCSPSigning
         */
            gen.AddExtension(
                X509Extensions.ExtendedKeyUsage.Id,
                false,
                new ExtendedKeyUsage(new[] { KeyPurposeID.IdKPServerAuth }));


            var newCert = gen.Generate(kpgen.GenerateKeyPair().Private);

            return new X509Certificate2(newCert.GetEncoded());
        }
    }
}