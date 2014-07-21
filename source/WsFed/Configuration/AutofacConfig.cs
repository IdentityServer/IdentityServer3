/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using Autofac.Integration.WebApi;
using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Hosting;
using Thinktecture.IdentityServer.WsFederation.ResponseHandling;
using Thinktecture.IdentityServer.WsFederation.Services;
using Thinktecture.IdentityServer.WsFederation.Validation;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    internal static class AutofacConfig
    {
        static ILog Logger = LogProvider.GetCurrentClassLogger();

        public static IContainer Configure(WsFederationPluginOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            var factory = options.Factory;
            factory.Validate();

            var builder = new ContainerBuilder();

            // mandatory from factory
            builder.Register(factory.UserService);
            builder.Register(factory.RelyingPartyService);
            builder.Register(factory.WsFederationSettings);

            // validators
            builder.RegisterType<SignInValidator>().AsSelf();

            // processors
            builder.RegisterType<SignInResponseGenerator>().AsSelf();
            builder.RegisterType<MetadataResponseGenerator>().AsSelf();
            
            // general services
            builder.RegisterType<CookieMiddlewareTrackingCookieService>().As<ITrackingCookieService>();
            builder.RegisterInstance(options).AsSelf();
            builder.RegisterInstance(options.IdentityServerOptions).AsSelf();

            // load core controller
            builder.RegisterApiControllers(typeof(WsFederationController).Assembly);

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