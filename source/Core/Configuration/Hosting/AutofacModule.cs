/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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
using Microsoft.Owin;
using Thinktecture.IdentityServer.Core.Endpoints;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    /// <summary>
    /// Autofac module which register Identity Server components. 
    /// This module can be used 
    /// </summary>
    public class AutofacModule : Module
    {
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Creates a new instance of the Autofac module.
        /// </summary>
        /// <param name="options">Identity server options</param>
        public AutofacModule(IdentityServerOptions options)
        {
            _options = options;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var options = _options;
            var fact = options.Factory;

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

            builder.RegisterDecoratorDefaultType<IEventService, EventServiceDecorator, DefaultEventService>(fact.EventService);

            builder.RegisterDefaultType<IRedirectUriValidator, DefaultRedirectUriValidator>(fact.RedirectUriValidator);
            builder.RegisterDefaultType<ILocalizationService, DefaultLocalizationService>(fact.LocalizationService);
            builder.RegisterDefaultType<IClientPermissionsService, DefaultClientPermissionsService>(fact.ClientPermissionsService);
            builder.RegisterDefaultType<IClientSecretValidator, HashedClientSecretValidator>(fact.ClientSecretValidator);

            if (fact.ViewService == null)
            {
                fact.ViewService = new DefaultViewServiceRegistration();
            }
            builder.Register(fact.ViewService);

            if (fact.CorsPolicyService == null)
            {
                fact.CorsPolicyService = new Registration<ICorsPolicyService>(new DefaultCorsPolicyService(options.CorsPolicy ?? new CorsPolicy()));
            }
            builder.Register(fact.CorsPolicyService);

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

            // validators
            builder.RegisterType<TokenRequestValidator>();
            builder.RegisterType<AuthorizeRequestValidator>();
            builder.RegisterType<ClientValidator>();
            builder.RegisterType<TokenValidator>();
            builder.RegisterType<EndSessionRequestValidator>();
            builder.RegisterType<BearerTokenUsageValidator>();
            builder.RegisterType<ScopeValidator>();
            builder.RegisterType<TokenRevocationRequestValidator>();

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

            // other internal
            builder.Register(c => new OwinEnvironmentService(c.Resolve<IOwinContext>()));
            builder.Register(c => new SessionCookie(c.Resolve<IOwinContext>(), c.Resolve<IdentityServerOptions>()));
            builder.Register(c => new MessageCookie<SignInMessage>(c.Resolve<IOwinContext>(), c.Resolve<IdentityServerOptions>()));
            builder.Register(c => new MessageCookie<SignOutMessage>(c.Resolve<IOwinContext>(), c.Resolve<IdentityServerOptions>()));
            builder.Register(c => new LastUserNameCookie(c.Resolve<IOwinContext>(), c.Resolve<IdentityServerOptions>()));
            builder.Register(c => new AntiForgeryToken(c.Resolve<IOwinContext>(), c.Resolve<IdentityServerOptions>()));

            // add any additional dependencies from hosting application
            foreach (var registration in fact.Registrations)
            {
                builder.Register(registration, registration.Name);
            }
        }
    }
}
