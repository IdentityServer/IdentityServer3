using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Owin;
using Sample;

[assembly: OwinStartup(typeof(MvcOwinWsFederation.Startup))]

namespace MvcOwinWsFederation
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions
                {
                    MetadataAddress = Constants.BaseAddress + "/wsfed/metadata",
                    Wtrealm = "urn:owinrp",

                    SignInAsAuthenticationType = "Cookies"
                });
        }
    }
}
