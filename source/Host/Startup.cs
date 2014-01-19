using Autofac;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Connect.TestServices;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.TestServices;

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
                    coreApp.UseIdentityServerCore(new IdentityServerCoreOptions{
                        Factory = TestOptionsFactory.Create()
                    });
                });
        }

        //private static void ConfigureMembershipReboot(IAppBuilder app, IContainer container)
        //{
        //    Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());
            
        //    var cookieOptions = new CookieAuthenticationOptions
        //    {
        //        AuthenticationType = "idsrv",
        //        //AuthenticationMode = AuthenticationMode.Passive,
        //        CookieSecure = CookieSecureOption.SameAsRequest
        //    };

        //    var appInfo = new OwinApplicationInformation(
        //        app,
        //        "Test",
        //        "Test Email Signature",
        //        "/Login",
        //        "/Register/Confirm/",
        //        "/Register/Cancel/",
        //        "/PasswordReset/Confirm/");

        //    var config = new MembershipRebootConfiguration<UserAccount>();
        //    config.RequireAccountVerification = false;

        //    config.AddCommandHandler((MapClaimsFromAccount<UserAccount> cmd) =>
        //        {
        //            cmd.MappedClaims = new Claim[]
        //                {
        //                    new Claim(Constants.ClaimTypes.Subject, cmd.Account.ID.ToString())
        //                };
        //        }
        //    );
        //    var emailFormatter = new EmailMessageFormatter(appInfo);
        //    // uncomment if you want email notifications -- also update smtp settings in web.config
        //    //config.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));

        //    var builder = new ContainerBuilder();

        //    builder.RegisterInstance(config).As<MembershipRebootConfiguration<UserAccount>>();

        //    builder.RegisterType<DefaultUserAccountRepository>()
        //        .As<IUserAccountRepository<UserAccount>>()
        //        .As<IUserAccountQuery>()
        //        .InstancePerLifetimeScope();

        //    builder.RegisterType<UserAccountService<UserAccount>>().OnActivating(e =>
        //    {
        //        var owin = e.Context.Resolve<IOwinContext>();
        //        var debugging = false;
        //        #if DEBUG
        //            debugging = true;
        //        #endif
        //        e.Instance.ConfigureTwoFactorAuthenticationCookies(owin.Environment, debugging);
        //    })
        //    .AsSelf()
        //    .InstancePerLifetimeScope();

        //    builder.Register(ctx =>
        //    {
        //        var owin = ctx.Resolve<IOwinContext>();
        //        return new OwinAuthenticationService<UserAccount>(
        //            cookieOptions.AuthenticationType, 
        //            ctx.Resolve<UserAccountService<UserAccount>>(), 
        //            owin.Environment);
        //    })
        //    .As<AuthenticationService<UserAccount>>()
        //    .InstancePerLifetimeScope();

        //    app.UseMembershipReboot(cookieOptions);

        //    builder.Update(container);
        //}
    }
}