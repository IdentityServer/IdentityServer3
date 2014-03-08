using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Admin.Core;

namespace MembershipReboot.IdentityServer.Admin
{
    public class MembershipRebootUserManager : IUserManager
    {
        public Task<Result> CreateAsync(string username, IEnumerable<System.Security.Claims.Claim> claims)
        {
            return Task.FromResult(Result.Success);
        }
    }
}
