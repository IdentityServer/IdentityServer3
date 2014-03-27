using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.TestServices;

[assembly: OwinStartup(typeof(Thinktecture.IdentityServer.Host.Startup))]
namespace Thinktecture.IdentityServer.Host
{
    public class ExternalConfigBase
    {
    }
    public class ExternalConfig<TOptions> : ExternalConfigBase
        where TOptions : Microsoft.Owin.Security.AuthenticationOptions
    {
        public ExternalConfig(TOptions options, Action<IAppBuilder> config)
        {
            this.Options = options;
            this.Config = config;
        }

        public TOptions Options { get; set; }
        public Action<IAppBuilder> Config { get; set; }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // idsrv core
            app.Map("/core", coreApp =>
                {
                    var factory = TestOptionsFactory.Create();
                    factory.UserService = MembershipReboot.IdentityServer.UserServiceFactory.Factory;

                    coreApp.UseIdentityServerCore(new IdentityServerCoreOptions
                    {
                        Factory = factory
                    },
                    (appCtx, signInAs) =>
                    {
                        var google = new GoogleAuthenticationOptions
                        {
                            AuthenticationType = "Google",
                            SignInAsAuthenticationType = signInAs
                        };
                        appCtx.UseGoogleAuthentication(google);

                        var fb = new FacebookAuthenticationOptions
                        {
                            AuthenticationType = "Facebook",
                            SignInAsAuthenticationType = signInAs,
                            AppId = "676607329068058",
                            AppSecret = "9d6ab75f921942e61fb43a9b1fc25c63"
                        };
                        appCtx.UseFacebookAuthentication(fb);
                    });

                    coreApp.Map("/foo", fooApp =>
                    {
                        fooApp.Run(async ctx =>
                        {
                            var r = await ctx.Authentication.AuthenticateAsync(Thinktecture.IdentityServer.Core.Constants.RedirectAuthenticationType);
                            await ctx.Response.WriteAsync("<h1>" + r.Identity.Name + "</h1><a href='resume'>resume</a>");
                        });

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