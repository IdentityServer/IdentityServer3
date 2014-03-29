using Owin;
using System;
using System.IO;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.ExceptionHandling;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Configuration
{
    class WebApiConfig
    {
        public static void Configure(IAppBuilder app, IDependencyResolver resolver, IdentityServerUserAdminConfiguration config)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (resolver == null) throw new ArgumentNullException("resolver");
            if (config == null) throw new ArgumentNullException("config");

            var apiConfig = new HttpConfiguration();
            apiConfig.MapHttpAttributeRoutes();
            apiConfig.DependencyResolver = resolver;

            apiConfig.SuppressDefaultHostAuthentication();
            apiConfig.Filters.Add(new HostAuthenticationAttribute("Bearer"));
            //apiConfig.Filters.Add(new AuthorizeAttribute(){Roles=config.AdminRoleName});

            apiConfig.Formatters.Remove(apiConfig.Formatters.XmlFormatter);
            apiConfig.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            apiConfig.Services.Add(typeof(IExceptionLogger), new UserAdminExceptionLogger());

            app.UseWebApi(apiConfig);
        }

        public class UserAdminExceptionLogger : ExceptionLogger
        {
            public override void Log(ExceptionLoggerContext context)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                path = Path.Combine(path, "UserAdminException.txt");
                Directory.CreateDirectory(path);
                var msg = DateTime.Now.ToString() + Environment.NewLine + context.Exception.ToString() + Environment.NewLine + Environment.NewLine;
                File.AppendAllText(path, msg);
            }
        }
    }
}
