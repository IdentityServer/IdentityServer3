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


using IdentityModel;
using IdentityServer3.Core.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer3.Tests.Endpoints.Connect.PoP
{
    static class Helper
    {
        public static RSACryptoServiceProvider CreateProvider(int keySize = 2048)
        {
            var csp = new CspParameters
            {
                Flags = CspProviderFlags.CreateEphemeralKey,
                KeyNumber = (int)KeyNumber.Signature
            };

            return new RSACryptoServiceProvider(keySize, csp);
        }

        public static RsaPublicKeyJwk CreateJwk()
        {
            var prov = CreateProvider();
            var pubKey = prov.ExportParameters(false);

            var jwk = new RsaPublicKeyJwk("key1")
            {
                kty = "RSA",
                n = Base64Url.Encode(pubKey.Modulus),
                e = Base64Url.Encode(pubKey.Exponent)
            };

            return jwk;
        }

        public static string CreateJwkString(RsaPublicKeyJwk jwk = null)
        {
            if (jwk == null) jwk = CreateJwk();

            var json = JsonConvert.SerializeObject(jwk);
            return Base64Url.Encode(Encoding.ASCII.GetBytes(json));
        }
    }
}