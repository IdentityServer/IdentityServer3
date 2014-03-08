using Autofac;
using Autofac.Integration.WebApi;
using MembershipReboot.IdentityServer.Admin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
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
                    UserManagerFactory = MembershipRebootUserManagerFactory.Create
                });
            });
        }
    }
}