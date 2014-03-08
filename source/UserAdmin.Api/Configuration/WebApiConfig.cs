using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Configuration
{
    class WebApiConfig
    {
        public static void Configure(IAppBuilder app, IDependencyResolver resolver)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (resolver == null) throw new ArgumentNullException("resolver");

            var apiConfig = new HttpConfiguration();
            apiConfig.DependencyResolver = resolver;
            apiConfig.SuppressDefaultHostAuthentication();
            apiConfig.Filters.Add(new HostAuthenticationAttribute("Bearer"));
            apiConfig.MapHttpAttributeRoutes();
            apiConfig.Formatters.Remove(apiConfig.Formatters.XmlFormatter);
            apiConfig.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            app.UseWebApi(apiConfig);
        }
    }
}
