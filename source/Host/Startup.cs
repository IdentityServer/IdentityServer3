using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Thinktecture.IdentityServer.Core;

[assembly: OwinStartup(typeof(Thinktecture.IdentityServer.Host.Startup))]
namespace Thinktecture.IdentityServer.Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // idsrv core
            app.Map("/core", coreApp =>
                {
                    coreApp.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationMode = AuthenticationMode.Passive,
                        AuthenticationType = "idsrv",
                        CookieSecure = CookieSecureOption.SameAsRequest
                    });

                    coreApp.UseWebApi(WebApiConfig.Configure());
                });
        }
    }
}