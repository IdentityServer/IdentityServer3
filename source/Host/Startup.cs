using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
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
                    coreApp.UseFileServer(new FileServerOptions
                    {
                        RequestPath = new PathString("/assets"),
                        FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Authentication.Assets")
                    });
                    coreApp.UseStageMarker(PipelineStage.MapHandler);
                    coreApp.UseFileServer(new FileServerOptions
                    {
                        RequestPath = new PathString("/assets/libs/fonts"),
                        FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Authentication.Assets.libs.bootstrap.fonts")
                    });
                    coreApp.UseStageMarker(PipelineStage.MapHandler);
                    
                    //coreApp.UseCookieAuthentication(new CookieAuthenticationOptions
                    //{
                    //    AuthenticationMode = AuthenticationMode.Passive,
                    //    AuthenticationType = "idsrv",
                    //    CookieSecure = CookieSecureOption.SameAsRequest
                    //});

                    // this needs to be before web api (where it's used)
                    ConfigureMembershipReboot(coreApp);
                    
                    coreApp.UseWebApi(WebApiConfig.Configure());
                });
        }

        private static void ConfigureMembershipReboot(IAppBuilder app)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());
            var cookieOptions = new CookieAuthenticationOptions
            {
                AuthenticationType = "idsrv",
                //AuthenticationMode = AuthenticationMode.Passive,
                CookieSecure = CookieSecureOption.SameAsRequest
            };
            Func<IDictionary<string, object>, UserAccountService<UserAccount>> uaFunc = env =>
            {
                var appInfo = new OwinApplicationInformation(
                    env,
                    "Test",
                    "Test Email Signature",
                    "/Login",
                    "/Register/Confirm/",
                    "/Register/Cancel/",
                    "/PasswordReset/Confirm/");

                var config = new MembershipRebootConfiguration<UserAccount>();
                var emailFormatter = new EmailMessageFormatter(appInfo);
                // uncomment if you want email notifications -- also update smtp settings in web.config
                //config.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));

                var svc = new UserAccountService<UserAccount>(config, new DefaultUserAccountRepository());
                var debugging = false;
#if DEBUG
                debugging = true;
#endif
                svc.ConfigureTwoFactorAuthenticationCookies(env, debugging);
                return svc;
            };
            Func<IDictionary<string, object>, AuthenticationService<UserAccount>> authFunc = env =>
            {
                return new OwinAuthenticationService<UserAccount>(cookieOptions.AuthenticationType, uaFunc(env), env);
            };

            app.UseMembershipReboot(cookieOptions, uaFunc, authFunc);
        }
    }
}