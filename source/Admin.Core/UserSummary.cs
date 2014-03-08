using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public class UserSummary
    {
        public string Subject { get; set; }
        public string Username { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
