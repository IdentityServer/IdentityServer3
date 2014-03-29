using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public class UserResult
    {
        public string Subject { get; set; }
        public string Username { get; set; }
        public IEnumerable<UserClaim> Claims { get; set; }
    }
}
