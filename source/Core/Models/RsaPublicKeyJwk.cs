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


namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Models an RSA public key JWK
    /// </summary>
    public class RsaPublicKeyJwk
    {
        /// <summary>
        /// key type
        /// </summary>
        public string kty { get; set; }

        /// <summary>
        /// modulus
        /// </summary>
        public string n { get; set; }

        /// <summary>
        /// exponent
        /// </summary>
        public string e { get; set; }

        /// <summary>
        /// algorithm
        /// </summary>
        public string alg { get; set; }

        /// <summary>
        /// key identifier
        /// </summary>
        public string kid { get; set; }

        /// <summary>
        /// Initializes the JWK with a key id
        /// </summary>
        /// <param name="kid"></param>
        public RsaPublicKeyJwk(string kid)
        {
            alg = "RS256";
            this.kid = kid;
        }
    }
}