using Autofac.Integration.WebApi;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerCoreOptions options)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();

            var resolver = new AutofacWebApiDependencyResolver(AutoFacConfig.Configure(options.Factory));
            config.DependencyResolver = resolver;

            return config;
        }
    }
}
