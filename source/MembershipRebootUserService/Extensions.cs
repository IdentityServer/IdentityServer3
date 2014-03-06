using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MembershipRebootUserService
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
