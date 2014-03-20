using System.Net.Http.Formatting;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerCoreOptions options)
        {
            var config = new HttpConfiguration();
            
            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();
            
            config.MessageHandlers.Insert(0, new KatanaDependencyResolver());

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            // todo
            //var logger = config.DependencyResolver.GetService(typeof(ILogger)) as ILogger;
            //config.Filters.Add(new ExceptionFilter(logger));

            return config;
        }
    }
}