using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Admin.Core;

namespace MembershipReboot.IdentityServer.Admin
{
    public class MembershipRebootUserManagerFactory
    {
        public static IUserManager Create()
        {
            return new MembershipRebootUserManager();
        }
    }
}
