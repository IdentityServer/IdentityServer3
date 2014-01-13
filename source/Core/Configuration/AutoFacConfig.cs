using Autofac;
using Autofac.Integration.WebApi;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Repositories;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Repositories;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core
{
    public static class AutoFacConfig
    {
        public static IContainer Configure(IdentityServerServiceFactory factory)
        {
            var builder = new ContainerBuilder();

            // validators
            builder.RegisterType<TokenRequestValidator>();
            builder.RegisterType<AuthorizeRequestValidator>();

            // processors
            builder.RegisterType<TokenResponseGenerator>();
            builder.RegisterType<AuthorizeResponseGenerator>();
            builder.RegisterType<AuthorizeInteractionResponseGenerator>();

            // repositories
            builder.RegisterType<InMemoryClientsRepository>().As<IClientsRepository>();
            builder.RegisterType<InMemorySettingsRepository>().As<ISettingsRepository>();

            // configuration
            builder.RegisterType<Configuration>();

            // services
            builder.RegisterType<ClientsService>().As<IClientsService>();
            builder.RegisterType<SettingsService>().As<ISettingsService>();
            builder.RegisterType<TestAuthorizationCodeService>().As<IAuthorizationCodeService>();
            builder.RegisterType<TestConsentService>().As<IConsentService>();
            //builder.RegisterType<TraceSourceLogger>().As<ILogger>();
            //builder.RegisterType<DebugLogger>().As<ILogger>();
            builder.Register(ctx => factory.Logger()).As<ILogger>();
            builder.RegisterType<TestTokenHandleService>().As<ITokenHandleService>();
            builder.RegisterType<DefaultTokenService>().As<ITokenService>();
            builder.RegisterType<TestProfileService>().As<IProfileService>();

            // controller
            builder.RegisterApiControllers(typeof(AuthorizeEndpointController).Assembly);

            // authN
            builder.RegisterType<Thinktecture.IdentityServer.Core.Authentication.AuthenticationService>().As<Thinktecture.IdentityServer.Core.Authentication.IAuthenticationService>();

            return builder.Build();
        }
    }
}