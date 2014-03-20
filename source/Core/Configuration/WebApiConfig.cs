using System.Net.Http.Formatting;
using System.Web.Http;

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

            return config;
        }
    }
}