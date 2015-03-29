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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer3.Core.Extensions
{
    internal static class X509Certificate2Extensions
    {
        public static bool IsPrivateAccessAllowed(this X509Certificate2 cert)
        {
            try
            {
                using (var pk = cert.PrivateKey)
                {
                    return true;
                }
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
