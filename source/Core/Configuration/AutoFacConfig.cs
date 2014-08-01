/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using Autofac.Integration.WebApi;
using System;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.Views.Embedded;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    internal static class AutofacConfig
    {
        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public static IContainer Configure(IdentityServerOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (options.Factory == null) throw new InvalidOperationException("null factory");

            IdentityServerServiceFactory fact = options.Factory;
            fact.Validate();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(options).AsSelf();

            // mandatory from factory
            builder.Register(fact.UserService);
            builder.Register(fact.ScopeStore);
            builder.Register(fact.ClientStore);
            
            // optional from factory
            if (fact.AuthorizationCodeStore != null)
            {
                builder.Register(fact.AuthorizationCodeStore);
            }
            else
            {
                var inmemCodeStore = new InMemoryAuthorizationCodeStore();
                builder.RegisterInstance(inmemCodeStore).As<IAuthorizationCodeStore>();
            }

            if (fact.TokenHandleStore != null)
            {
                builder.Register(fact.TokenHandleStore);
            }
            else
            {
                var inmemTokenHandleStore = new InMemoryTokenHandleStore();
                builder.RegisterInstance(inmemTokenHandleStore).As<ITokenHandleStore>();
            }

            if (fact.RefreshTokenStore != null)
            {
                builder.Register(fact.RefreshTokenStore);
            }
            else
            {
                var inmemRefreshTokenStore = new InMemoryRefreshTokenStore();
                builder.RegisterInstance(inmemRefreshTokenStore).As<IRefreshTokenStore>();
            }

            if (fact.ConsentStore != null)
            {
                builder.Register(fact.ConsentStore);
            }
            else
            {
                var inmemConsentStore = new InMemoryConsentStore();
                builder.RegisterInstance(inmemConsentStore).As<IConsentStore>();
            }

            if (fact.ClaimsProvider != null)
            {
                builder.Register(fact.ClaimsProvider);
            }
            else
            {
                builder.RegisterType<DefaultClaimsProvider>().As<IClaimsProvider>();
            }

            if (fact.TokenService != null)
            {
                builder.Register(fact.TokenService);
            }
            else
            {
                builder.RegisterType<DefaultTokenService>().As<ITokenService>();
            }

            if (fact.RefreshTokenService != null)
            {
                builder.Register(fact.RefreshTokenService);
            }
            else
            {
                builder.RegisterType<DefaultRefreshTokenService>().As<IRefreshTokenService>();
            }

            if (fact.TokenSigningService != null)
            {
                builder.Register(fact.TokenSigningService);
            }
            else
            {
                builder.RegisterType<DefaultTokenSigningService>().As<ITokenSigningService>();
            }

            if (fact.CustomRequestValidator != null)
            {
                builder.Register(fact.CustomRequestValidator);
            }
            else
            {
                builder.RegisterType<DefaultCustomRequestValidator>().As<ICustomRequestValidator>();
            }

            if (fact.AssertionGrantValidator != null)
            {
                builder.Register(fact.AssertionGrantValidator);
            }
            else
            {
                builder.RegisterType<DefaultAssertionGrantValidator>().As<IAssertionGrantValidator>();
            }

            if (fact.ExternalClaimsFilter != null)
            {
                builder.Register(fact.ExternalClaimsFilter);
            }
            else
            {
                builder.RegisterType<DefaultExternalClaimsFilter>().As<IExternalClaimsFilter>();
            }

            if (fact.CustomTokenValidator != null)
            {
                builder.Register(fact.CustomTokenValidator);
            }
            else
            {
                builder.RegisterType<DefaultCustomTokenValidator>().As<ICustomTokenValidator>();
            }
            
            if (fact.ConsentService != null)
            {
                builder.Register(fact.ConsentService);
            }
            else
            {
                builder.RegisterType<DefaultConsentService>().As<IConsentService>();
            }

            if (fact.ViewService != null)
            {
                builder.Register(fact.ViewService);
            }
            else
            {
                builder.RegisterType<EmbeddedAssetsViewService>().As<IViewService>();
            }

            // validators
            builder.RegisterType<TokenRequestValidator>();
            builder.RegisterType<AuthorizeRequestValidator>();
            builder.RegisterType<ClientValidator>();
            builder.RegisterType<TokenValidator>();

            // processors
            builder.RegisterType<TokenResponseGenerator>();
            builder.RegisterType<AuthorizeResponseGenerator>();
            builder.RegisterType<AuthorizeInteractionResponseGenerator>();
            builder.RegisterType<UserInfoResponseGenerator>();

            // general services
            builder.RegisterType<CookieMiddlewareTrackingCookieService>().As<ITrackingCookieService>();

            // for authentication
            var authenticationOptions = options.AuthenticationOptions ?? new AuthenticationOptions();
            builder.RegisterInstance(authenticationOptions).AsSelf();

            // load core controller
            builder.RegisterApiControllers(typeof(AuthorizeEndpointController).Assembly);

            // add any additional dependencies from hosting application
            foreach(var registration in fact.Registrations)
            {
                builder.Register(registration);
            }

            return builder.Build();
        }

        private static void Register(this ContainerBuilder builder, Registration registration)
        {
            if (registration.ImplementationType != null)
            {
                builder.RegisterType(registration.ImplementationType).As(registration.InterfaceType);
            }
            else if (registration.ImplementationFactory != null)
            {
                builder.Register(ctx => registration.ImplementationFactory()).As(registration.InterfaceType);
            }
            else
            {
                var message = "No type or factory found on registration " + registration.GetType().FullName; 
                Logger.Error(message);
                throw new InvalidOperationException(message);
            }
        }
    }
}