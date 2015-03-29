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

using System.IdentityModel;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer3.Core.Configuration
{
    /// <summary>
    /// X.509 certificate based data protector
    /// </summary>
    public class X509CertificateDataProtector : IDataProtector
    {
        readonly CookieTransform _encrypt;
        readonly CookieTransform _sign;

        /// <summary>
        /// Initializes a new instance of the <see cref="X509CertificateDataProtector"/> class.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        public X509CertificateDataProtector(X509Certificate2 certificate)
        {
            _encrypt = new RsaEncryptionCookieTransform(certificate);
            _sign = new RsaSignatureCookieTransform(certificate);
        }

        /// <summary>
        /// Protects the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public byte[] Protect(byte[] data, string entropy = "")
        {
            var encrypted = _encrypt.Encode(data);
            return _sign.Encode(encrypted);
        }

        /// <summary>
        /// Unprotects the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns></returns>
        public byte[] Unprotect(byte[] data, string entropy = "")
        {
            var validated = _sign.Decode(data);
            return _encrypt.Decode(validated);
        }
    }
}