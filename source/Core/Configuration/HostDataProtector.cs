/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.Owin.Security.DataProtection;
namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class HostDataProtector : IDataProtector
    {
        private IDataProtectionProvider _provider;

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