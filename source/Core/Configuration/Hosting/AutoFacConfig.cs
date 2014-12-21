/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Autofac;
using Autofac.Integration.WebApi;
using System;
using Thinktecture.IdentityServer.Core.Endpoints;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
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
            builder.Register(fact.ScopeStore);
            builder.Register(fact.ClientStore);
            builder.RegisterDecorator<IUserService, ExternalClaimsFilterUserService>(fact.UserService);
            
            // optional from factory
            builder.RegisterDecoratorDefaultInstance<IAuthorizationCodeStore, KeyHashingAuthorizationCodeStore, InMemoryAuthorizationCodeStore>(fact.AuthorizationCodeStore);
            builder.RegisterDecoratorDefaultInstance<ITokenHandleStore, KeyHashingTokenHandleStore, InMemoryTokenHandleStore>(fact.TokenHandleStore);
            builder.RegisterDecoratorDefaultInstance<IRefreshTokenStore, KeyHashingRefreshTokenStore, InMemoryRefreshTokenStore>(fact.RefreshTokenStore);
            builder.RegisterDefaultInstance<IConsentStore, InMemoryConsentStore>(fact.ConsentStore);
            builder.RegisterDefaultType<IClaimsProvider, DefaultClaimsProvider>(fact.ClaimsProvider);
            builder.RegisterDefaultType<ITokenService, DefaultTokenService>(fact.TokenService);
            builder.RegisterDefaultType<IRefreshTokenService, DefaultRefreshTokenService>(fact.RefreshTokenService);
            builder.RegisterDefaultType<ITokenSigningService, DefaultTokenSigningService>(fact.TokenSigningService);
            builder.RegisterDefaultType<ICustomRequestValidator, DefaultCustomRequestValidator>(fact.CustomRequestValidator);
            builder.RegisterDefaultType<ICustomGrantValidator, DefaultCustomGrantValidator>(fact.CustomGrantValidator);
            builder.RegisterDefaultType<IExternalClaimsFilter, NopClaimsFilter>(fact.ExternalClaimsFilter);
            builder.RegisterDefaultType<ICustomTokenValidator, DefaultCustomTokenValidator>(fact.CustomTokenValidator);
            builder.RegisterDefaultType<IConsentService, DefaultConsentService>(fact.ConsentService);
            builder.RegisterDefaultType<IEventService, DefaultEventService>(fact.EventService);
            builder.RegisterDefaultType<IRedirectUriValidator, DefaultRedirectUriValidator>(fact.RedirectUriValidator);
            builder.RegisterDefaultType<ILocalizationService, DefaultLocalizationService>(fact.LocalizationService);
            builder.RegisterDefaultType<IClientPermissionsService, DefaultClientPermissionsService>(fact.ClientPermissionsService);
            builder.RegisterDefaultType<IViewService, DefaultViewService>(fact.ViewService);
            
            // this is more of an internal interface, but maybe we want to open it up as pluggable?
            // this is used by the DefaultClientPermissionsService below, or it could be used
            // by a custom IClientPermissionsService
            builder.Register(ctx =>
            {
                var consent = ctx.Resolve<IConsentStore>();
                var refresh = ctx.Resolve<IRefreshTokenStore>();
                var code = ctx.Resolve<IAuthorizationCodeStore>();
                var access = ctx.Resolve<ITokenHandleStore>();
                return new AggregatePermissionsStore(
                    consent,
                    new TokenMetadataPermissionsStoreAdapter(refresh.GetAllAsync, refresh.RevokeAsync),
                    new TokenMetadataPermissionsStoreAdapter(code.GetAllAsync, code.RevokeAsync),
                    new TokenMetadataPermissionsStoreAdapter(access.GetAllAsync, access.RevokeAsync)
                );
            }).As<IPermissionsStore>();

            // hosting services
            builder.RegisterType<OwinEnvironmentService>();

            // validators
            builder.RegisterType<TokenRequestValidator>();
            builder.RegisterType<AuthorizeRequestValidator>();
            builder.RegisterType<ClientValidator>();
            builder.RegisterType<TokenValidator>();
            builder.RegisterType<EndSessionRequestValidator>();
            builder.RegisterType<BearerTokenUsageValidator>();
            builder.RegisterType<ScopeValidator>();

            // processors
            builder.RegisterType<TokenResponseGenerator>();
            builder.RegisterType<AuthorizeResponseGenerator>();
            builder.RegisterType<AuthorizeInteractionResponseGenerator>();
            builder.RegisterType<UserInfoResponseGenerator>();
            builder.RegisterType<EndSessionResponseGenerator>();

            // for authentication
            var authenticationOptions = options.AuthenticationOptions ?? new AuthenticationOptions();
            builder.RegisterInstance(authenticationOptions).AsSelf();

            // load core controller
            builder.RegisterApiControllers(typeof(AuthorizeEndpointController).Assembly);

            // add any additional dependencies from hosting application
            foreach(var registration in fact.Registrations)
            {
                builder.Register(registration, registration.Name);
            }

            return builder.Build();
        }

        private static void RegisterDefaultType<T, TDefault>(this ContainerBuilder builder, Registration<T> registration, string name = null)
            where T : class
            where TDefault : T
        {
            if (registration != null)
            {
                builder.Register(registration, name);
            }
            else
            {
                if (name == null)
                {
                    builder.RegisterType<TDefault>().As<T>();
                }
                else
                {
                    builder.RegisterType<TDefault>().Named<T>(name);
                }
            }
        }
        
        private static void RegisterDefaultInstance<T, TDefault>(this ContainerBuilder builder, Registration<T> registration, string name = null)
            where T : class
            where TDefault : class, T, new()
        {
            if (registration != null)
            {
                builder.Register(registration, name);
            }
            else
            {
                if (name == null)
                {
                    builder.RegisterInstance(new TDefault()).As<T>();
                }
                else
                {
                    builder.RegisterInstance(new TDefault()).Named<T>(name);
                }
            }
        }

        private static void RegisterDecorator<T, TDecorator>(this ContainerBuilder builder, string name)
            where T : class
            where TDecorator : T
        {
            builder.RegisterType<TDecorator>();
            builder.Register<T>(ctx =>
            {
                var inner = Autofac.Core.ResolvedParameter.ForNamed<T>(name);
                return ctx.Resolve<TDecorator>(inner);
            });
        }
        
        private static void RegisterDecoratorDefaultInstance<T, TDecorator, TDefault>(this ContainerBuilder builder, Registration<T> registration)
            where T : class
            where TDefault : class, T, new()
        {
            builder.RegisterDefaultInstance<T, TDefault>(registration, "inner");
            builder.RegisterDecorator<T, TDefault>("inner");
        }

        private static void RegisterDecorator<T, TDecorator>(this ContainerBuilder builder, Registration<T> registration)
            where T : class
            where TDecorator : T
        {
            builder.Register(registration, "inner");
            builder.RegisterType<TDecorator>();
            builder.Register<T>(ctx =>
            {
                var inner = Autofac.Core.ResolvedParameter.ForNamed<T>("inner");
                return ctx.Resolve<TDecorator>(inner);
            });
        }
        
        private static void Register(this ContainerBuilder builder, Registration registration, string name = null)
        {
            if (registration.ImplementationType != null)
            {
                var reg = builder.RegisterType(registration.ImplementationType);
                if (name != null)
                {
                    reg.Named(name, registration.InterfaceType);
                }
                else
                {
                    reg.As(registration.InterfaceType);
                }
            }
            else if (registration.ImplementationFactory != null)
            {
                var reg = builder.Register(ctx => registration.ImplementationFactory(new AutofacDependencyResolver(ctx)));
                if (name != null)
                {
                    reg.Named(name, registration.InterfaceType);
                }
                else
                {
                    reg.As(registration.InterfaceType);
                }
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