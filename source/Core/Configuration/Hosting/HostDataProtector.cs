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

using Microsoft.Owin.Security.DataProtection;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class HostDataProtector : IDataProtector
    {
        private readonly IDataProtectionProvider _provider;

        public HostDataProtector(IDataProtectionProvider provider)
        {
            _provider = provider;
        }

        public byte[] Protect(byte[] data, string entropy = "")
        {
            var protector = _provider.Create(entropy);
            return protector.Protect(data);
        }

        public byte[] Unprotect(byte[] data, string entropy = "")
        {
            var protector = _provider.Create(entropy);
            return protector.Unprotect(data);
        }
    }
}