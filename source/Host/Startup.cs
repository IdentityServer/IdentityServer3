using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.TestServices;

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
                    var factory = TestOptionsFactory.Create();

                    factory.UserService = Thinktecture.IdentityServer.MembershipReboot.UserServiceFactory.Factory;
                    //factory.UserService = AspNetIdentity.IdentityServer.UserServiceFactory.Factory;
                    
                    var opts = new IdentityServerCoreOptions
                    {
                        Factory = factory
                    };
                    coreApp.UseIdentityServerCore(opts, ConfigureSocialIdentityProviders);
                });
        }

        public static void ConfigureSocialIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleAuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType
            };
            app.UseGoogleAuthentication(google);

            var fb = new FacebookAuthenticationOptions
            {
                AuthenticationType = "Facebook",
                SignInAsAuthenticationType = signInAsType,
                AppId = "676607329068058",
                AppSecret = "9d6ab75f921942e61fb43a9b1fc25c63"
            };
            app.UseFacebookAuthentication(fb);
        }
    }
}