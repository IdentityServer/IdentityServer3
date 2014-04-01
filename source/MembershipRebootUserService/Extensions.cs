/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;

namespace Thinktecture.IdentityServer.MembershipReboot
{
    static class Extensions
    {
        public static Guid ToGuid(this string s)
        {
            Guid g;
            if (!String.IsNullOrWhiteSpace(s) &&
                Guid.TryParse(s, out g))
            {
                return g;
            }
            
            return Guid.Empty;
        }
    }
}
