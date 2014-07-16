/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class FuncDataProtector : IDataProtector
    {
        readonly Func<byte[], string, byte[]> _protectFunc;
        readonly Func<byte[], string, byte[]> _unprotectFunc;

        public FuncDataProtector(
            Func<byte[], string, byte[]> protectFunc,
            Func<byte[], string, byte[]> unprotectFunc)
        {
            _protectFunc = protectFunc;
            _unprotectFunc = unprotectFunc;
        }

        public byte[] Protect(byte[] data, string entropy = "")
        {
            return _protectFunc(data, entropy);
        }

        public byte[] Unprotect(byte[] data, string entropy = "")
        {
            return _unprotectFunc(data, entropy);
        }
    }
}