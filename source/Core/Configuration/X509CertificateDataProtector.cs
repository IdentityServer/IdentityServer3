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
using System.IdentityModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
            //as there is no way to include entropy as separate attribute or flag we just append it to the end of the data
            //to be able to take it into consideration when unprotecting
            var entropyBytes = GetBytes(entropy);
            var dataWithEntropy = Combine(data, entropyBytes);

            var encrypted = _encrypt.Encode(dataWithEntropy);
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
            var decoded = _encrypt.Decode(validated);

            //need to reverse things done in protect before returning: subtract entropy from the end and ensure it matches
            var entropyBytes = GetBytes(entropy);
            var decodedEntropy = new byte[entropyBytes.Length];
            var decodedDataLength = decoded.Length - entropyBytes.Length;
            Array.Copy(decoded, decodedDataLength, decodedEntropy, 0, entropyBytes.Length);

            var rez = decodedEntropy.SequenceEqual(entropyBytes) ? GetSubArray(decoded, decodedDataLength) : null;
            return rez;
        }

        private static byte[] GetBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        private static byte[] GetSubArray(byte[] src, int length)
        {
            var dst = new byte[length];
            Array.Copy(src, dst, length);
            return dst;
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            var combined = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, combined, 0, first.Length);
            Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);
            return combined;
        }
    }
}