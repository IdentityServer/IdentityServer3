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

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class DataProtectorAdapter : Microsoft.Owin.Security.DataProtection.IDataProtector
    {
        private readonly IDataProtector _idsrvProtector;
        private readonly string _entropy;

        public DataProtectorAdapter(IDataProtector idsrvProtector, string entropy)
        {
            _idsrvProtector = idsrvProtector;
            _entropy = entropy;
        }

        public byte[] Protect(byte[] userData)
        {
            return _idsrvProtector.Protect(userData, _entropy);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return _idsrvProtector.Unprotect(protectedData, _entropy);
        }
    }
}
