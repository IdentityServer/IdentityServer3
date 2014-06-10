using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Thinktecture.IdentityServer.AspNetIdentity
{
    public class CustomDbContext : IdentityDbContext<CustomUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomDbContext(string connString)
            : base(connString)
        {
        }
    }
    public class CustomUserStore : UserStore<CustomUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomUserStore(CustomDbContext ctx)
            : base(ctx)
        {
        }
    }
    public class CustomUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim> { }
    public class CustomRole : IdentityRole<int, CustomUserRole> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }
    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }

    public class CustomUserManager : UserManager<CustomUser, int>
    {
        public CustomUserManager(CustomUserStore store)
            : base(store)
        {
        }
    }
}
