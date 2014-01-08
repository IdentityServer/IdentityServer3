using Autofac;
using Autofac.Integration.WebApi;
using Thinktecture.IdentityServer.Core.Protocols.Connect;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Repositories;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Services;
using Thinktecture.IdentityServer.Core.Repositories;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core
{
    public static class AutoFacConfig
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            // validators
            builder.RegisterType<OidcTokenRequestValidator>();
            builder.RegisterType<OidcAuthorizeRequestValidator>();

            // processors
            builder.RegisterType<OidcTokenResponseGenerator>();
            builder.RegisterType<OidcAuthorizeResponseGenerator>();

            // repositories
            builder.RegisterType<InMemoryOidcClientsRepository>().As<IOidcClientsRepository>();
            builder.RegisterType<InMemorySettingsRepository>().As<ISettingsRepository>();

            // configuration
            builder.RegisterType<OidcConfiguration>();

            // services
            builder.RegisterType<OidcClientsService>().As<IOidcClientsService>();
            builder.RegisterType<SettingsService>().As<ISettingsService>();
            builder.RegisterType<TestAuthorizationCodeService>().As<IAuthorizationCodeService>();
            builder.RegisterType<TestConsentService>().As<IConsentService>();
            //builder.RegisterType<TraceSourceLogger>().As<ILogger>();
            builder.RegisterType<DebugLogger>().As<ILogger>();
            builder.RegisterType<TestTokenHandleService>().As<ITokenHandleService>();
            builder.RegisterType<DefaultOidcTokenService>().As<IOidcTokenService>();
            builder.RegisterType<TestProfileService>().As<IProfileService>();

            // controller
            builder.RegisterApiControllers(typeof(OidcAuthorizeEndpointController).Assembly);

            return builder.Build();
        }
    }
}