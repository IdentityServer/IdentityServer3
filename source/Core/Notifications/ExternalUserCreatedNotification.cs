using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thinktecture.IdentityServer.Core.Notifications
{
    public class ExternalUserCreatedNotification
    {
        public string Subject { get; set; }

        public ICollection<System.Security.Claims.Claim> RedirectClaims { get; set; }
    }
}
