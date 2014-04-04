using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Security.Cookies;
using System.IdentityModel.Tokens;
using Microsoft.Owin;

[assembly:OwinStartup(typeof(MVC_OWIN_Client.Startup))]

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

            app.UseOpenIdConnectAuthentication(new Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions
                {
                    Client_Id = "implicitclient",
                    Authority = "http://localhost:3333/core",
                    Redirect_Uri = "http://localhost:2671/",
                    Response_Type = "id_token",
                    Scope = "openid email",

                    SignInAsAuthenticationType = "Cookies"
                });
        }
    }
}