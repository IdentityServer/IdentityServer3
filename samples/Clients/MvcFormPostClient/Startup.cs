using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;

namespace MvcFormPostClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;

            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                });


            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "TempCookie",
                    AuthenticationMode = AuthenticationMode.Passive
                });
        }
    }
}