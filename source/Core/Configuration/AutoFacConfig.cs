/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using Autofac.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class AutofacConfig
    {
        public static IContainer Configure(IdentityServerCoreOptions options, InternalConfiguration internalConfig)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (options.Factory == null) throw new InvalidOperationException("null factory");
            if (internalConfig == null) throw new ArgumentNullException("internalConfig");

            IdentityServerServiceFactory fact = options.Factory;
            fact.Validate();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(internalConfig).AsSelf();

            // mandatory from factory
            builder.Register(ctx => fact.AuthorizationCodeStore()).As<IAuthorizationCodeStore>();
            builder.Register(ctx => fact.CoreSettings()).As<CoreSettings>();
            builder.Register(ctx => fact.TokenHandleStore()).As<ITokenHandleStore>();
            builder.Register(ctx => fact.UserService()).As<IUserService>();
            builder.Register(ctx => fact.ScopeService()).As<IScopeService>();
            builder.Register(ctx => fact.ClientService()).As<IClientService>();
            builder.Register(ctx => fact.ConsentService()).As<IConsentService>();

            // optional from factory
            if (fact.Logger != null)
            {
                builder.Register(ctx => fact.Logger()).As<ILogger>();
            }
            else
            {
                builder.RegisterType<TraceLogger>().As<ILogger>();
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

            // for authentication
            var authenticationOptions = options.AuthenticationOptions ?? new AuthenticationOptions();
            builder.RegisterInstance(authenticationOptions).AsSelf();

            // load core controller
            builder.RegisterApiControllers(typeof(AuthorizeEndpointController).Assembly);

            // plugin configuration
            var pluginDepencies = internalConfig.PluginDependencies;
            if (pluginDepencies != null)
            {
                if (pluginDepencies.ApiControllerAssemblies != null)
                {
                    foreach (var asm in pluginDepencies.ApiControllerAssemblies)
                    {
                        builder.RegisterApiControllers(asm);
                    }
                }

                if (pluginDepencies.Types != null)
                {
                    foreach (var type in pluginDepencies.Types)
                    {
                        if (type.Value == null)
                        {
                            builder.RegisterType(type.Key);
                        }
                        else
                        {
                            builder.RegisterType(type.Key).As(type.Value);
                        }
                    }
                }

                if (pluginDepencies.Factories != null)
                {
                    foreach (var factory in pluginDepencies.Factories)
                    {
                        builder.Register(ctx => factory.Value()).As(factory.Key);
                    }
                }
            }

            return builder.Build();
        }
    }
}