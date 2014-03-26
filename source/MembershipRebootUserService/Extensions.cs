using System;

namespace MembershipReboot.IdentityServer
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
