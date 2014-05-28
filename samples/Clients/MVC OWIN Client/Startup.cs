using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Sample;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(MVC_OWIN_Client.Startup))]

namespace MVC_OWIN_Client
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    Client_Id = "implicitclient",
                    Authority = Constants.BaseAddress,
                    Redirect_Uri = "http://localhost:2671/",
                    Response_Type = "id_token token",
                    Scope = "openid email",

                    SignInAsAuthenticationType = "Cookies",

                    // sample how to access token on form (for token response type)
                    //Notifications = new OpenIdConnectAuthenticationNotifications
                    //{
                    //    MessageReceived = async n =>
                    //        {
                    //            var token = n.ProtocolMessage.Token;

                    //            if (!string.IsNullOrEmpty(token))
                    //            {
                    //                n.OwinContext.Set<string>("idsrv:token", token);
                    //            }
                    //        },
                    //    SecurityTokenValidated = async n =>
                    //        {
                    //            var token = n.OwinContext.Get<string>("idsrv:token");

                    //            if (!string.IsNullOrEmpty(token))
                    //            {
                    //                n.AuthenticationTicket.Identity.AddClaim(
                    //                    new Claim("access_token", token));
                    //            }
                    //        }
                    //}
                });
        }
    }
}