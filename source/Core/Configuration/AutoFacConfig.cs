/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using Autofac.Integration.WebApi;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class OurModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
        }
    }

    public static class AutoFacConfig
    {
        public static IContainer Configure(IdentityServerServiceFactory fact)
        {
            fact.Validate();

            var builder = new ContainerBuilder();

            // mandatory from factory
            builder.Register(ctx => fact.AuthorizationCodeStore()).As<IAuthorizationCodeStore>();
            builder.Register(ctx => fact.CoreSettings()).As<ICoreSettings>();
            builder.Register(ctx => fact.Logger()).As<ILogger>();
            builder.Register(ctx => fact.TokenHandleStore()).As<ITokenHandleStore>();
            builder.Register(ctx => fact.UserService()).As<IUserService>();
            builder.Register(ctx => fact.ConsentService()).As<IConsentService>();

            // optional from factory
            if (fact.ClaimsProvider != null)
            {
                builder.Register(ctx => fact.ClaimsProvider()).As<IClaimsProvider>();
            }
            else
            {
                builder.RegisterType<DefaultClaimsProvider>().As<IClaimsProvider>();
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

            // validators
            builder.RegisterType<TokenRequestValidator>();
            builder.RegisterType<AuthorizeRequestValidator>();
            builder.RegisterType<UserInfoRequestValidator>();
            builder.RegisterType<ClientValidator>();

            // processors
            builder.RegisterType<TokenResponseGenerator>();
            builder.RegisterType<AuthorizeResponseGenerator>();
            builder.RegisterType<AuthorizeInteractionResponseGenerator>();
            builder.RegisterType<UserInfoResponseGenerator>();

            // services
            builder.RegisterType<DefaultTokenService>().As<ITokenService>();

            // controller
            builder.RegisterApiControllers(typeof(AuthorizeEndpointController).Assembly);

            return builder.Build();
        }
    }
}