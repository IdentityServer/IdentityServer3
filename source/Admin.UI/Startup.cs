using MembershipReboot.IdentityServer.Admin;
using Owin;
using Thinktecture.IdentityServer.UserAdmin.Api.Configuration;

namespace Thinktecture.IdentityServer.Admin.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/api", api =>
            {
                api.UseIdentityServerUserAdmin(new IdentityServerUserAdminConfiguration()
                {
                    //OAuthAuthorizationEndpoint = "/authorize",
                    //AdminRoleName = "Foo",
                    UserManagerFactory = MembershipRebootUserManagerFactory.Create
                });
            });
        }
    }
}