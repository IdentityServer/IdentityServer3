/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class CookieOptions
    {
        public string Prefix { get; set; }
        public TimeSpan ExpireTimeSpan { get; set; }
        public bool IsPersistent { get; set; }
    }
}
