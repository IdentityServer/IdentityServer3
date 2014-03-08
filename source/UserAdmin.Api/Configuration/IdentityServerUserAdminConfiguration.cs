using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Admin.Core;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Configuration
{
    public class IdentityServerUserAdminConfiguration
    {
        public Func<IUserManager> UserManagerFactory { get; set; }
    }
}
