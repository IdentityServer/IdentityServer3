/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class CookieOptions
    {
        public CookieOptions()
        {
            ExpireTimeSpan = Constants.DefaultCookieTimeSpan;
            SlidingExpiration = false;
        }

        public string Prefix { get; set; }
        public TimeSpan ExpireTimeSpan { get; set; }
        public bool IsPersistent { get; set; }
        public bool SlidingExpiration { get; set; }
    }
}
