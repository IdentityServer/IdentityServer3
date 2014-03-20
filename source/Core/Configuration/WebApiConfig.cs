using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Thinktecture.IdentityServer.Core.Plumbing;

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

            config.Services.Add(typeof(IExceptionLogger), new IdentityServerExceptionLogger());

            return config;
        }
    }
}