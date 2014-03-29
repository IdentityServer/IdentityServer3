using Autofac;
using Autofac.Integration.WebApi;
using System;
using System.Web.Http.Dependencies;
using Thinktecture.IdentityServer.Admin.Core;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Configuration
{
    class AutofacConfig
    {
        public static IDependencyResolver Configure(IdentityServerUserAdminConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");

            var builder = new ContainerBuilder();
            builder
                .Register(ctx => config.UserManagerFactory())
                .As<IUserManager>()
                .InstancePerApiRequest();
            builder
                .RegisterApiControllers(typeof(AutofacConfig).Assembly);
            
            var container = builder.Build();
            return new AutofacWebApiDependencyResolver(container);
        }
    }
}
