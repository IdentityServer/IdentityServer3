//using Microsoft.Owin;
//using Microsoft.Owin.Security;
//using Microsoft.Owin.Security.Cookies;
//using Owin;

//[assembly: OwinStartup(typeof(IdSrv3.Startup))]

//namespace IdSrv3
//{
//    public class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            app.UseCookieAuthentication(new CookieAuthenticationOptions
//                {
//                    AuthenticationMode = AuthenticationMode.Passive,
//                    AuthenticationType = "idsrv",
//                    CookieSecure = CookieSecureOption.SameAsRequest
//                });

//            app.UseWebApi(WebApiConfig.Configure());
//        }
//    }
//}