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
            builder.Register(fact.UserService, "inner");
            builder.RegisterDecorator<IUserService>((s, inner) =>
            {
                var filter = s.Resolve<IExternalClaimsFilter>();
                return new ExternalClaimsFilterUserService(filter, inner);
            }, "inner");

            builder.Register(fact.ScopeStore);
            builder.Register(fact.ClientStore);
            
            // optional from factory
            if (fact.AuthorizationCodeStore != null)
            {
                builder.Register(fact.AuthorizationCodeStore, "inner");
            }
            else
            {
                var inmemCodeStore = new InMemoryAuthorizationCodeStore();
                builder.RegisterInstance(inmemCodeStore).Named<IAuthorizationCodeStore>("inner");
            }
            builder.RegisterDecorator<IAuthorizationCodeStore>((s, inner) =>
            {
                return new KeyHashingAuthorizationCodeStore(inner);
            }, "inner");

            if (fact.TokenHandleStore != null)
            {
                builder.Register(fact.TokenHandleStore, "inner");
            }
            else
            {
                var inmemTokenHandleStore = new InMemoryTokenHandleStore();
                builder.RegisterInstance(inmemTokenHandleStore).Named<ITokenHandleStore>("inner");
            }
            builder.RegisterDecorator<ITokenHandleStore>((s, inner) =>
            {
                return new KeyHashingTokenHandleStore(inner);
            }, "inner");

            if (fact.RefreshTokenStore != null)
            {
                builder.Register(fact.RefreshTokenStore, "inner");
            }
            else
            {
                var inmemRefreshTokenStore = new InMemoryRefreshTokenStore();
                builder.RegisterInstance(inmemRefreshTokenStore).Named<IRefreshTokenStore>("inner");
            }
            builder.RegisterDecorator<IRefreshTokenStore>((s, inner) =>
            {
                return new KeyHashingRefreshTokenStore(inner);
            }, "inner");

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

            if (fact.CustomGrantValidator != null)
            {
                builder.Register(fact.CustomGrantValidator);
            }
            else
            {
                builder.RegisterType<DefaultCustomGrantValidator>().As<ICustomGrantValidator>();
            }

            if (fact.ExternalClaimsFilter != null)
            {
                builder.Register(fact.ExternalClaimsFilter);
            }
            else
            {
                builder.RegisterType<NopClaimsFilter>().As<IExternalClaimsFilter>();
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

            if (fact.ClientPermissionsService != null)
            {
                builder.Register(fact.ClientPermissionsService);
            }
            else
            {
                builder.RegisterType<DefaultClientPermissionsService>().As<IClientPermissionsService>();
            }

            if (fact.ViewService != null)
            {
                builder.Register(fact.ViewService);
            }
            else
            {
                builder.RegisterType<DefaultViewService>().As<IViewService>();
            }

            // validators
            builder.RegisterType<TokenRequestValidator>();
            builder.RegisterType<AuthorizeRequestValidator>();
            builder.RegisterType<ClientValidator>();
            builder.RegisterType<TokenValidator>();
            builder.RegisterType<EndSessionRequestValidator>();

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
                builder.Register(registration);
            }

            return builder.Build();
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
                var reg = builder.Register(ctx => registration.ImplementationFactory());
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