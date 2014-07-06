/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using Autofac.Integration.WebApi;
using System;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Hosting;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    internal static class AutofacConfig
    {
        public static IContainer Configure(IdentityServerOptions options, InternalConfiguration internalConfig)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (options.Factory == null) throw new InvalidOperationException("null factory");
            if (internalConfig == null) throw new ArgumentNullException("internalConfig");

            IdentityServerServiceFactory fact = options.Factory;
            fact.Validate();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(internalConfig).AsSelf();

            // mandatory from factory
            if (fact.UserService.Type != null)
            {
                builder.RegisterType(fact.UserService.Type).As<IUserService>();
            }
            else
            {
                builder.Register(ctx => fact.UserService.TypeFactory()).As<IUserService>();
            }
            
            builder.Register(ctx => fact.CoreSettings()).As<CoreSettings>();
            builder.Register(ctx => fact.ScopeService()).As<IScopeService>();
            builder.Register(ctx => fact.ClientService()).As<IClientService>();
            
            // optional from factory
            if (fact.AuthorizationCodeStore != null)
            {
                builder.Register(ctx => fact.AuthorizationCodeStore()).As<IAuthorizationCodeStore>();
            }
            else
            {
                var inmemCodeStore = new InMemoryAuthorizationCodeStore();
                builder.RegisterInstance(inmemCodeStore).As<IAuthorizationCodeStore>();
            }

            if (fact.TokenHandleStore != null)
            {
                builder.Register(ctx => fact.TokenHandleStore()).As<ITokenHandleStore>();
            }
            else
            {
                var inmemTokenHandleStore = new InMemoryTokenHandleStore();
                builder.RegisterInstance(inmemTokenHandleStore).As<ITokenHandleStore>();
            }

            if (fact.ConsentService != null)
            {
                builder.Register(ctx => fact.ConsentService()).As<IConsentService>();
            }
            else
            {
                var inmemConsentService = new InMemoryConsentService();
                builder.RegisterInstance(inmemConsentService).As<IConsentService>();
            }

            if (fact.ClaimsProvider != null)
            {
                builder.Register(ctx => fact.ClaimsProvider()).As<IClaimsProvider>();
            }
            else
            {
                builder.RegisterType<DefaultClaimsProvider>().As<IClaimsProvider>();
            }

            if (fact.TokenService != null)
            {
                builder.Register(ctx => fact.TokenService()).As<ITokenService>();
            }
            else
            {
                builder.RegisterType<DefaultTokenService>().As<ITokenService>();
            }

            if (fact.CustomRequestValidator != null)
            {
                builder.Register(ctx => fact.CustomRequestValidator()).As<ICustomRequestValidator>();
            }
            else
            {
                builder.RegisterType<DefaultCustomRequestValidator>().As<ICustomRequestValidator>();
            }

            if (fact.AssertionGrantValidator != null)
            {
                builder.Register(ctx => fact.AssertionGrantValidator()).As<IAssertionGrantValidator>();
            }
            else
            {
                builder.RegisterType<DefaultAssertionGrantValidator>().As<IAssertionGrantValidator>();
            }

            if (fact.ExternalClaimsFilter != null)
            {
                builder.Register(ctx => fact.ExternalClaimsFilter()).As<IExternalClaimsFilter>();
            }
            else
            {
                builder.RegisterType<DefaultExternalClaimsFilter>().As<IExternalClaimsFilter>();
            }

            if (fact.CustomTokenValidator != null)
            {
                builder.Register(ctx => fact.CustomTokenValidator()).As<ICustomTokenValidator>();
            }
            else
            {
                builder.RegisterType<DefaultCustomTokenValidator>().As<ICustomTokenValidator>();
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

            return builder.Build();
        }
    }
}